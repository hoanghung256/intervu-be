# cs_parser.py
# Parse C# xUnit test file → List[TestCase] using regex (no Roslyn needed)

import re
from dataclasses import dataclass, field
from typing import List, Dict, Tuple, Optional

from mapping_rules import (
    HTTP_STATUS_MAP, BASE_TEST_CONSTANTS, AUTH_METHOD_MAP,
    SETUP_DATA_PATTERNS, BOUNDARY_KEYWORDS, ABNORMAL_KEYWORDS,
    SKIP_ASSERT_MESSAGE_PATTERNS,
)


@dataclass
class TestCase:
    index: int
    method_name: str

    # API call
    http_method: str = 'GET'
    endpoint: str = ''
    payload_fields: List[Tuple[str, str]] = field(default_factory=list)
    query_params: Dict[str, str] = field(default_factory=dict)
    is_multipart: bool = False
    multipart_filename: str = ''
    multipart_content_type: str = ''
    main_call_has_jwt: bool = False  # True if main API call has jwtToken: arg

    # Expected result
    expected_status: int = 200
    expected_status_text: str = '200 OK'
    assert_messages: List[str] = field(default_factory=list)

    # Precondition inference
    requires_auth: bool = False
    auth_role: str = 'Public'
    auth_description: str = ''
    has_setup_data: bool = False
    resource_not_exists: bool = False

    # Classification
    test_type: str = 'N'   # N = Normal, A = Abnormal, B = Boundary


class CSharpTestParser:
    """
    Parse C# xUnit [Fact] test methods from a .cs file using regex.
    Skips [Fact(Skip="...")] methods.
    """

    # ── Regex patterns ──────────────────────────────────────────────────────

    # [Fact] or [Fact(Skip="...")] possibly followed by [Trait("Key","Val")] lines
    _RE_FACT = re.compile(
        r'\[Fact(?P<skip_part>\(Skip\s*=\s*"[^"]*"\))?\]'
        r'(?:\s*\[[^\]]+\])*'                                    # any number of [Attribute] 
        r'\s+public\s+(?:async\s+)?Task(?:<[^>]+>)?\s+'
        r'(?P<method>\w+)\s*\(',
        re.MULTILINE | re.DOTALL,
    )

    # var response/updateResponse/deleteResponse/etc = await _api.XxxAsync(
    _RE_MAIN_CALL = re.compile(
        r'\bvar\s+\w*[Rr]esponse\s*=\s*await\s+_api\s*\.\s*'
        r'(?P<verb>Get|Post|Put|Delete|Patch)(?P<multi>Multipart)?Async\s*\(',
        re.IGNORECASE,
    )

    # Fallback: any await _api.XxxAsync( that is NOT preceded by var login/var xxx = pattern
    # (i.e. not stored in a 'login', 'create', 'token' variable)
    _RE_ANY_CALL = re.compile(
        r'\bawait\s+_api\s*\.\s*'
        r'(?P<verb>Get|Post|Put|Delete|Patch)(?P<multi>Multipart)?Async\s*\(',
        re.IGNORECASE,
    )

    # Skip-variable names that indicate a setup call, not the main call
    _SETUP_VAR_NAMES = re.compile(
        r'var\s+(login|create|token|setup|register|user|id)\w*\s*=\s*await\s+_api',
        re.IGNORECASE,
    )

    # AssertHelper.AssertEqual(HttpStatusCode.XXX,
    _RE_STATUS = re.compile(r'AssertEqual\s*\(\s*HttpStatusCode\.(?P<code>\w+)')

    # AssertHelper.AssertXxx(..., ..., "message") or AssertHelper.AssertXxx(..., "message")
    _RE_ASSERT_MSG = re.compile(
        r'Assert(?:Helper\s*\.)?\s*Assert(?:Equal|True|False|NotNull|NotEmpty|Null)\s*\('
        r'(?:[^"]*?,\s*)*"(?P<msg>[^"]+)"'   # last string arg = message
        r'\s*\)',
        re.DOTALL,
    )

    # new XxxRequest/XxxDto { field = val, ... }  — handles fully-qualified names with dots
    _RE_NEW_OBJ = re.compile(
        r'new\s+[\w.]+(?:Request|Dto|Body|Model|Data)\s*[\n\s]*\{(?P<fields>[^{}]*)\}',
        re.DOTALL,
    )

    # FieldName = value (inside new {...})
    _RE_FIELD = re.compile(
        r'(?P<name>\w+)\s*=\s*(?P<val>(?:"[^"]*"|\$"[^"]*"|[\w.<>\[\],\s]+?))\s*(?:,|$)',
        re.MULTILINE,
    )

    def parse_file(self, filepath: str) -> List[TestCase]:
        with open(filepath, 'r', encoding='utf-8-sig') as f:
            content = f.read()
        return self._parse(content)

    # ── Main parse loop ──────────────────────────────────────────────────────

    def _parse(self, content: str) -> List[TestCase]:
        results: List[TestCase] = []
        idx = 1

        for m in self._RE_FACT.finditer(content):
            is_skip = m.group('skip_part') is not None
            method_name = m.group('method')

            if is_skip:
                continue

            body = self._extract_body(content, m.end())
            tc = TestCase(index=idx, method_name=method_name)

            self._extract_api_call(body, tc)
            self._extract_asserts(body, tc)
            self._infer_preconditions(body, tc)
            self._infer_test_type(method_name, tc)

            results.append(tc)
            idx += 1

        return results

    # ── Body extraction (brace counting) ────────────────────────────────────

    def _extract_body(self, content: str, start: int) -> str:
        """Return text between the first matched { } pair after `start`."""
        pos = start
        while pos < len(content) and content[pos] != '{':
            pos += 1
        if pos >= len(content):
            return ''

        depth = 0
        body_start = pos + 1
        i = pos
        while i < len(content):
            ch = content[i]
            if ch == '{':
                depth += 1
            elif ch == '}':
                depth -= 1
                if depth == 0:
                    return content[body_start:i]
            i += 1
        return ''

    # ── API call extraction ──────────────────────────────────────────────────

    def _extract_api_call(self, body: str, tc: TestCase):
        """
        Find the MAIN _api.XxxAsync call — the one whose response is asserted.
        Priority:
          1. var *Response = await _api.XxxAsync(
          2. Last await _api.XxxAsync( that is NOT a setup-variable assignment
        """
        m = self._RE_MAIN_CALL.search(body)
        if not m:
            # Find all _api calls; skip ones assigned to setup variables
            setup_positions = {fm.start() for fm in self._SETUP_VAR_NAMES.finditer(body)}
            for fm in self._RE_ANY_CALL.finditer(body):
                # Check if this call is part of a setup-variable assignment
                # by looking ~60 chars before the match for 'var xxx =' pattern
                context_before = body[max(0, fm.start() - 60):fm.start()]
                is_setup = bool(re.search(
                    r'var\s+(?:login|create|token|setup|register|user|id)\w*\s*=\s*$',
                    context_before, re.IGNORECASE
                ))
                if not is_setup:
                    m = fm
                    break   # take first non-setup call
            if not m:
                return

        tc.http_method = m.group('verb').upper()
        tc.is_multipart = bool(m.group('multi'))

        # Extract args text (balanced parentheses)
        call_args = self._extract_paren_content(body, m.end() - 1)

        self._parse_url(call_args, tc)

        if tc.http_method in ('POST', 'PUT', 'PATCH') and not tc.is_multipart:
            self._parse_payload(call_args, body, tc)

        # Auth status is derived from whether THE MAIN CALL has a jwtToken argument
        tc.main_call_has_jwt = 'jwtToken:' in call_args

        if tc.is_multipart:
            strings = re.findall(r'"([^"]+)"', call_args)
            for s in strings:
                if '.' in s and '/' not in s and len(s) < 30:
                    tc.multipart_filename = s
                elif '/' in s and len(s) < 30 and s.count('/') == 1:
                    tc.multipart_content_type = s

    def _extract_paren_content(self, text: str, open_pos: int) -> str:
        """Return content between ( and matching )."""
        if open_pos < len(text) and text[open_pos] != '(':
            p = text.find('(', open_pos)
            if p == -1:
                return ''
            open_pos = p

        depth = 0
        i = open_pos
        while i < len(text):
            if text[i] == '(':
                depth += 1
            elif text[i] == ')':
                depth -= 1
                if depth == 0:
                    return text[open_pos + 1:i]
            i += 1
        return ''

    # ── URL parsing ──────────────────────────────────────────────────────────

    def _parse_url(self, call_args: str, tc: TestCase):
        """Extract endpoint + query params from the first string arg."""
        # $"... interpolated ..." or "..." literal
        m = re.match(r'\s*\$?"(?P<url>[^"]*)"', call_args)
        if not m:
            tc.endpoint = '/api/v1/...'
            return

        raw_url = m.group('url')
        # Normalise C# interpolation {expr} → {id}
        raw_url = re.sub(r'\{[^}]+\}', '{id}', raw_url)

        if '?' in raw_url:
            base, qs = raw_url.split('?', 1)
            tc.endpoint = base
            for part in qs.split('&'):
                if '=' in part:
                    k, v = part.split('=', 1)
                    tc.query_params[k] = v
        else:
            tc.endpoint = raw_url

    # ── Payload parsing ──────────────────────────────────────────────────────

    def _parse_payload(self, call_args: str, body: str, tc: TestCase):
        """Extract fields from new XxxRequest { ... } in call args or body."""
        # Try call_args first, then full body
        for text in (call_args, body):
            m = self._RE_NEW_OBJ.search(text)
            if m:
                self._parse_fields(m.group('fields'), tc)
                return

    def _parse_fields(self, fields_text: str, tc: TestCase):
        for fm in self._RE_FIELD.finditer(fields_text):
            name = fm.group('name').strip()
            raw_val = fm.group('val').strip().rstrip(',').strip()
            # Skip C# keywords that appear as "names"
            if name in ('new', 'var', 'true', 'false', 'null', 'await', 'return'):
                continue
            resolved = self._resolve_value(raw_val)
            tc.payload_fields.append((name, resolved))

    def _resolve_value(self, raw: str) -> str:
        raw = raw.strip()
        # String literal
        if raw.startswith('"') and raw.endswith('"') and len(raw) >= 2:
            return raw[1:-1]
        # Interpolated string
        if raw.startswith('$"') and raw.endswith('"'):
            inner = re.sub(r'\{[^}]+\}', '<dynamic>', raw[2:-1])
            return inner
        # Known base constants
        if raw in BASE_TEST_CONSTANTS:
            return BASE_TEST_CONSTANTS[raw]
        # Numeric
        if re.match(r'^[\d.]+$', raw):
            return raw
        # null / true / false
        if raw in ('null', 'true', 'false'):
            return raw
        # List / array
        list_m = re.match(r'new\s+(?:List<[^>]+>|[\w\[\]]+)\s*\{([^}]*)\}', raw, re.DOTALL)
        if list_m:
            items = [x.strip().strip('"') for x in list_m.group(1).split(',') if x.strip()]
            return f'[{", ".join(items)}]'
        # Enum: SomeEnum.Value → Value
        if '.' in raw and not raw.startswith('"'):
            return raw.split('.')[-1]
        # Variable name heuristics
        lower = raw.lower()
        if 'email' in lower:
            return '<email@example.com>'
        if ('guid' in lower or lower in ('id', 'userid', 'coachid', 'candidateid')):
            return '<uuid>'
        if 'token' in lower:
            return '<jwt_token>'
        if 'password' in lower:
            return '<password>'
        if re.match(r'^[a-zA-Z_]\w*$', raw):
            return f'<{raw}>'
        return raw

    # ── Assert extraction ────────────────────────────────────────────────────

    def _extract_asserts(self, body: str, tc: TestCase):
        # HTTP status
        for m in self._RE_STATUS.finditer(body):
            code_name = m.group('code')
            if code_name in HTTP_STATUS_MAP:
                tc.expected_status, tc.expected_status_text = HTTP_STATUS_MAP[code_name]
            break  # first AssertEqual = the status assertion

        # Assert messages (useful log hints)
        for m in self._RE_ASSERT_MSG.finditer(body):
            msg = m.group('msg')
            # Skip generic "Status code is 200 OK" style messages
            if any(re.match(pat, msg, re.IGNORECASE) for pat in SKIP_ASSERT_MESSAGE_PATTERNS):
                continue
            if msg not in tc.assert_messages:
                tc.assert_messages.append(msg)

    # ── Precondition inference ───────────────────────────────────────────────

    def _infer_preconditions(self, body: str, tc: TestCase):
        """
        Auth precondition is inferred from whether the MAIN API call itself
        passes a jwtToken (tc.main_call_has_jwt), NOT just from whether any
        Login helper appears in the body (they may be there only for setup).

        Auth ROLE is still inferred from the Login helper method used.
        """
        # Detect auth role from setup/login helper calls
        found_auth_role: str = 'Public'
        found_auth_desc: str = ''
        for pattern, (role, desc) in AUTH_METHOD_MAP:
            if pattern in body:
                found_auth_role = role
                found_auth_desc = desc
                break

        # Only mark as 'requires_auth' if the MAIN call passes the token
        if tc.main_call_has_jwt:
            tc.requires_auth = True
            tc.auth_role = found_auth_role
            tc.auth_description = found_auth_desc or f'{found_auth_role} is authenticated'
        elif 'jwtToken:' in body:
            # Some OTHER call in the body has jwtToken (setup), main call doesn't
            tc.requires_auth = False
            tc.auth_role = 'Public'

        # DB setup data (CreateTestUser, etc.)
        if any(p in body for p in SETUP_DATA_PATTERNS):
            tc.has_setup_data = True
        # Also treat CreateTestUserAsync as setup data indicator
        if 'CreateTestUserAsync' in body or 'CreateTestUser' in body:
            tc.has_setup_data = True

        # Resource intentionally missing
        lower_name = tc.method_name.lower()
        if any(kw in lower_name for kw in ('notexist', 'doesnotexist', 'nonexistent', 'notfound')):
            tc.resource_not_exists = True

    # ── Test type inference ──────────────────────────────────────────────────

    def _infer_test_type(self, method_name: str, tc: TestCase):
        lower = method_name.lower()
        for kw in BOUNDARY_KEYWORDS:
            if kw.lower() in lower:
                tc.test_type = 'B'
                return
        for kw in ABNORMAL_KEYWORDS:
            if kw.lower() in lower:
                tc.test_type = 'A'
                return
        tc.test_type = 'N'

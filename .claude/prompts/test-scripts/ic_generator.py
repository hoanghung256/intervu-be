# ic_generator.py
# Convert List[TestCase] -> IcSheetContent (all rows, all marks)

from dataclasses import dataclass, field
from typing import List, Dict, Optional

from cs_parser import TestCase


@dataclass
class IcRow:
    """
    Represents one row in the IC sheet.

    Layout mapping:
      col A (1)  = section_label   e.g. "Condition", "Confirm", "Result"
      col B (2)  = sub_label       e.g. "Precondition", "Input", "Return"
      col D (4)  = value           e.g. "API server is reachable", "200 OK"
      col F+ (6+)= marks[utcid]    "O", "N", "A", "B", "P", or a date string

    For header rows (is_header=True): value is empty, marks is empty.
    For Result rows: value is empty, marks contain N/A/B or "P" or date.
    """
    section_label: str = ''
    sub_label: str = ''
    value: str = ''
    marks: Dict[int, str] = field(default_factory=dict)   # {1-based utcid index -> value}
    is_header: bool = False   # True for sub-section header rows


@dataclass
class IcSheetContent:
    sheet_name: str         # "IC-20"
    function_name: str      # "Contribute to Question Bank"
    n_cases: int

    created_by: str = ''
    executed_by: str = ''
    executed_date: str = ''
    lines_of_code: int = 0

    rows: List[IcRow] = field(default_factory=list)

    @property
    def passed_count(self) -> int:
        return self.n_cases   # all test cases pass

    @property
    def n_normal(self) -> int:
        return sum(1 for r in self.rows
                   if r.sub_label == 'Type(N : Normal, A : Abnormal, B : Boundary)'
                   for v in r.marks.values() if v == 'N')

    @property
    def n_abnormal(self) -> int:
        return sum(1 for r in self.rows
                   if r.sub_label == 'Type(N : Normal, A : Abnormal, B : Boundary)'
                   for v in r.marks.values() if v == 'A')

    @property
    def n_boundary(self) -> int:
        return sum(1 for r in self.rows
                   if r.sub_label == 'Type(N : Normal, A : Abnormal, B : Boundary)'
                   for v in r.marks.values() if v == 'B')


class IcContentGenerator:
    """
    Given a list of TestCase objects, produce the complete IcSheetContent
    that describes exactly what to write into the Excel sheet.
    """

    def generate(
        self,
        test_cases: List[TestCase],
        sheet_name: str,
        function_name: str,
        created_by: str = '',
        executed_by: str = '',
        executed_date: str = '',
        lines_of_code: int = 0,
    ) -> IcSheetContent:

        content = IcSheetContent(
            sheet_name=sheet_name,
            function_name=function_name,
            n_cases=len(test_cases),
            created_by=created_by,
            executed_by=executed_by,
            executed_date=executed_date,
            lines_of_code=lines_of_code,
        )

        content.rows += self._gen_condition_section(test_cases)
        content.rows += self._gen_confirm_section(test_cases)
        content.rows += self._gen_result_section(test_cases)
        return content

    # ── Helpers ──────────────────────────────────────────────────────────────

    def _all(self, n: int) -> Dict[int, str]:
        """O mark for every test case."""
        return {i: 'O' for i in range(1, n + 1)}

    def _subset(self, indices: List[int]) -> Dict[int, str]:
        """O mark for a specific subset of test cases."""
        return {i: 'O' for i in indices}

    # ── CONDITION section ─────────────────────────────────────────────────────

    def _gen_condition_section(self, tcs: List[TestCase]) -> List[IcRow]:
        rows: List[IcRow] = []
        n = len(tcs)

        # ── Precondition sub-section ───────────────────────────────────────
        rows.append(IcRow(section_label='Condition', sub_label='Precondition', is_header=True))

        # 1. API server always reachable -> all
        rows.append(IcRow(value='API server is reachable', marks=self._all(n)))

        # 2. Auth preconditions - group by (role, description)
        auth_groups: Dict[tuple, List[int]] = {}
        for tc in tcs:
            if tc.requires_auth:
                key = (tc.auth_role, tc.auth_description or f'{tc.auth_role} account is authenticated')
                auth_groups.setdefault(key, []).append(tc.index)

        for (role, desc), indices in auth_groups.items():
            rows.append(IcRow(value=desc, marks=self._subset(indices)))

        # Also mark non-authenticated test cases explicitly if mixed auth exists
        public_tcs = [tc.index for tc in tcs if not tc.requires_auth]
        if public_tcs and auth_groups:
            rows.append(IcRow(value='No authentication required (public endpoint)',
                              marks=self._subset(public_tcs)))

        # 3. Resource intentionally not existing
        no_res = [tc.index for tc in tcs if tc.resource_not_exists]
        if no_res:
            rows.append(IcRow(value='Resource does not exist in database',
                              marks=self._subset(no_res)))

        # 4. Complex DB setup data needed
        setup = [tc.index for tc in tcs if tc.has_setup_data]
        if setup:
            rows.append(IcRow(value='Test data exists in database',
                              marks=self._subset(setup)))

        # ── Input sub-section ──────────────────────────────────────────────
        rows.append(IcRow(sub_label='Input', is_header=True))

        for tc in tcs:
            inp = self._format_input(tc)
            rows.append(IcRow(value=inp, marks={tc.index: 'O'}))

        # ── HTTP Method sub-section ────────────────────────────────────────
        rows.append(IcRow(sub_label='HTTP Method', is_header=True))

        # Group by method (usually the same, but handle edge cases)
        method_groups: Dict[str, List[int]] = {}
        for tc in tcs:
            method_groups.setdefault(tc.http_method, []).append(tc.index)

        for method, indices in method_groups.items():
            rows.append(IcRow(value=method, marks=self._subset(indices)))

        # ── API Endpoint sub-section ───────────────────────────────────────
        rows.append(IcRow(sub_label='API Endpoint', is_header=True))

        endpoint_groups: Dict[str, List[int]] = {}
        for tc in tcs:
            endpoint_groups.setdefault(tc.endpoint, []).append(tc.index)

        for ep, indices in endpoint_groups.items():
            rows.append(IcRow(value=ep, marks=self._subset(indices)))

        return rows

    # ── CONFIRM section ───────────────────────────────────────────────────────

    def _gen_confirm_section(self, tcs: List[TestCase]) -> List[IcRow]:
        rows: List[IcRow] = []

        # ── Return sub-section ─────────────────────────────────────────────
        rows.append(IcRow(section_label='Confirm', sub_label='Return', is_header=True))

        status_groups: Dict[tuple, List[int]] = {}
        for tc in tcs:
            key = (tc.expected_status, tc.expected_status_text)
            status_groups.setdefault(key, []).append(tc.index)

        for (code, text), indices in sorted(status_groups.items()):
            rows.append(IcRow(value=text, marks=self._subset(indices)))

        # ── Exception sub-section ─────────────────────────────────────────
        # Tests don't directly verify exception types -> header only
        rows.append(IcRow(sub_label='Exception', is_header=True))

        # ── Log message sub-section ───────────────────────────────────────
        rows.append(IcRow(sub_label='Log message', is_header=True))

        msg_groups: Dict[str, List[int]] = {}
        for tc in tcs:
            for msg in tc.assert_messages:
                msg_groups.setdefault(msg, []).append(tc.index)

        for msg, indices in msg_groups.items():
            rows.append(IcRow(value=f'"{msg}"', marks=self._subset(indices)))

        return rows

    # ── RESULT section ────────────────────────────────────────────────────────

    def _gen_result_section(self, tcs: List[TestCase]) -> List[IcRow]:
        rows: List[IcRow] = []

        # Type row: N / A / B per UTCID column
        type_marks = {tc.index: tc.test_type for tc in tcs}
        rows.append(IcRow(
            section_label='Result',
            sub_label='Type(N : Normal, A : Abnormal, B : Boundary)',
            marks=type_marks,
        ))

        # Passed/Failed row
        rows.append(IcRow(
            sub_label='Passed/Failed',
            marks={tc.index: 'P' for tc in tcs},
        ))

        # Executed Date row
        rows.append(IcRow(
            sub_label='Executed Date',
            marks={tc.index: '__DATE__' for tc in tcs},
        ))

        # Defect ID row (empty)
        rows.append(IcRow(sub_label='Defect ID'))

        return rows

    # ── Input formatting ──────────────────────────────────────────────────────

    def _format_input(self, tc: TestCase) -> str:
        """Format a human-readable input string for each test case."""
        if tc.is_multipart:
            parts = [f'file: {tc.multipart_filename or "<file>"}']
            if tc.multipart_content_type:
                parts.append(f'type: {tc.multipart_content_type}')
            return '{ ' + ', '.join(parts) + ' }'

        if tc.payload_fields:
            parts = []
            for k, v in tc.payload_fields:
                # Don't quote non-string values
                if v in ('null', 'true', 'false') or v.startswith('[') or v.startswith('<'):
                    parts.append(f'{k}: {v}')
                else:
                    parts.append(f'{k}: "{v}"')
            return '{ ' + ', '.join(parts) + ' }'

        if tc.query_params:
            parts = [f'{k}: {v}' for k, v in tc.query_params.items()]
            return '{ ' + ', '.join(parts) + ' }'

        # Endpoint-only GET with no body
        return f'(no body - {tc.http_method} {tc.endpoint})'

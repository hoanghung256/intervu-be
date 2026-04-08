# mapping_rules.py
# Lookup tables dùng cho IC sheet auto-filler

# HttpStatusCode enum name → (int code, display text)
HTTP_STATUS_MAP = {
    'OK':                    (200, '200 OK'),
    'Created':               (201, '201 Created'),
    'NoContent':             (204, '204 No Content'),
    'BadRequest':            (400, '400 Bad Request'),
    'Unauthorized':          (401, '401 Unauthorized'),
    'Forbidden':             (403, '403 Forbidden'),
    'NotFound':              (404, '404 Not Found'),
    'MethodNotAllowed':      (405, '405 Method Not Allowed'),
    'Conflict':              (409, '409 Conflict'),
    'UnprocessableEntity':   (422, '422 Unprocessable Entity'),
    'TooManyRequests':       (429, '429 Too Many Requests'),
    'InternalServerError':   (500, '500 Internal Server Error'),
    'ServiceUnavailable':    (503, '503 Service Unavailable'),
}

# BaseTest constants (d:\Coding\dotnet source\PRN_FinalProject\intervu-be\Intervu.API.Test\Base\BaseTest.cs)
BASE_TEST_CONSTANTS = {
    'CANDIDATE_PASSWORD': 'abc@12345',
    'DEFAULT_PASSWORD':   '123',
    'ADMIN_EMAIL':        'admin@example.com',
    'COACH_EMAIL':        'coach@example.com',
}

# Method name fragment → (role label, precondition description)
# Longer/more specific patterns first for greedy matching
AUTH_METHOD_MAP = [
    ('LoginAdminAsync',             ('Admin',     'Admin account is authenticated (valid JWT)')),
    ('LoginAdmin',                  ('Admin',     'Admin account is authenticated (valid JWT)')),
    ('LoginAsAliceAsync',           ('Candidate', 'Candidate (Alice) account exists and is authenticated')),
    ('LoginAsAlice',                ('Candidate', 'Candidate (Alice) account exists and is authenticated')),
    ('RegisterAndLoginUserAsync',   ('Candidate', 'User account is registered and authenticated')),
    ('RegisterAndLogin',            ('Candidate', 'User account is registered and authenticated')),
    ('LoginAsCandidateAsync',       ('Candidate', 'Candidate account is authenticated')),
    ('LoginAsCandidate',            ('Candidate', 'Candidate account is authenticated')),
    ('LoginUserAsync',              ('Candidate', 'User account is authenticated')),
    ('LoginUser',                   ('Candidate', 'User account is authenticated')),
    ('LoginAsCoachAsync',           ('Coach',     'Coach account is authenticated')),
    ('LoginAsCoach',                ('Coach',     'Coach account is authenticated')),
    ('LoginCoachAsync',             ('Coach',     'Coach account is authenticated')),
    ('LoginCoach',                  ('Coach',     'Coach account is authenticated')),
    ('CreateCoachAndServiceAsync',  ('Coach',     'Coach account with interview service exists')),
    ('CreateCoachAndService',       ('Coach',     'Coach account with interview service exists')),
    ('CreateTestInterviewRoom',     ('Candidate', 'Interview booking and room exist in database')),
]

# Patterns indicating complex DB setup data is needed
SETUP_DATA_PATTERNS = [
    'SetupTestData', 'CreateTestInterviewRoom', 'CreateTestQuestion',
    'CreateCoach', 'SetupTestRoom', 'CreateTestData', 'CreateBooking',
    'CreateAndLogin', 'SetupData',
]

# Keywords → Boundary test type (check first — priority over Abnormal)
BOUNDARY_KEYWORDS = [
    'Null', 'Empty', 'Zero', 'Minimum', 'Maximum', 'Boundary',
    'MissingField', 'EmptyBody', 'EmptyRequest', 'NullInput',
    'EmptyString', 'WhiteSpace',
]

# Keywords → Abnormal test type
ABNORMAL_KEYWORDS = [
    'BadRequest', 'Invalid', 'Incorrect', 'Wrong', 'Unauthorized',
    'Forbidden', 'NotFound', 'DoesNotExist', 'NonExistent', 'Expired',
    'Mismatch', 'WhenNot', 'WhenFailed', 'WhenError', 'NoAuth',
    'WithoutToken', 'WhenMissing', 'WhenInvalid', 'WhenIncorrect',
    'WhenWrong', 'WhenUnauthorized', 'WhenForbidden', 'Failure',
]

# Assert messages that are too generic to be useful as log entries
SKIP_ASSERT_MESSAGE_PATTERNS = [
    r'^Status code is \d',
    r'^Response (status|code)',
    r'^HTTP status',
]

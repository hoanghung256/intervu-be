# ic_filler.py
# Write IcSheetContent into an openpyxl Workbook

import openpyxl
from openpyxl.styles import Alignment, Border, Side, Font, PatternFill
from openpyxl.utils import get_column_letter

from ic_generator import IcSheetContent, IcRow

# ── Fixed row positions (1-based, matching real IC template) ─────────────────
ROW_FUNC_CODE   = 2
ROW_CREATED_BY  = 3
ROW_LINES_CODE  = 4
ROW_TEST_REQ    = 5
ROW_STATS_HDR   = 6
ROW_STATS       = 7
ROW_UTCID       = 9
ROW_DATA_START  = 10

# ── Fixed column positions (1-based) ────────────────────────────────────────
COL_A   = 1   # Section label:  "Condition", "Confirm", "Result"
COL_B   = 2   # Sub-label:      "Precondition", "Input", "Return", …
COL_D   = 4   # Value:          "API server is reachable", "200 OK", …
COL_F   = 6   # First UTCID column (UTCID01)

# Metadata layout
COL_IC_CODE_VAL    = 3   # C: "IC-20"
COL_FUNC_NAME_LBL  = 6   # F: "Function Name"
COL_FUNC_NAME_VAL  = 12  # L: actual function name string
COL_EXEC_LBL       = 6   # F: "Executed By"
COL_EXEC_VAL       = 12  # L: executed-by name
COL_TOTAL_TC       = 15  # O: "Total Test Cases" / count

# ── Thin border helper ────────────────────────────────────────────────────────
_THIN = Side(style='thin')
_BORDER = Border(left=_THIN, right=_THIN, top=_THIN, bottom=_THIN)


class IcSheetFiller:
    """
    Fills the target Excel sheet with IcSheetContent.
    If the sheet already has O marks -> skip (do not overwrite).
    """

    # ── Public entry point ────────────────────────────────────────────────────

    def fill(self, wb: openpyxl.Workbook, content: IcSheetContent) -> bool:
        """
        Returns True  -> sheet was filled and workbook should be saved.
        Returns False -> sheet was already filled; nothing changed.

        Strategy:
          - Sheet EXISTS  -> clear only cell VALUES (keep all style/colors/borders),
                             then write values-only (no _apply_styles call).
          - Sheet MISSING -> create new sheet, write content + apply styles.
        """
        name = content.sheet_name

        if name in wb.sheetnames:
            ws = wb[name]
            if self._is_filled(ws):
                print(f'  -> Skip: sheet "{name}" already has content (O marks found).')
                return False
            # Sheet exists but is empty: clear only VALUES, keep all formatting
            self._clear_values_only(ws)
            self._write_metadata(ws, content)
            self._write_stats(ws, content)
            self._write_utcid_headers(ws, content.n_cases)
            last_row = self._write_data_rows(ws, content)
            # NOTE: _apply_styles is NOT called — existing sheet style is preserved
        else:
            # Brand-new sheet: create with full style
            ws = wb.create_sheet(name)
            self._write_metadata(ws, content)
            self._write_stats(ws, content)
            self._write_utcid_headers(ws, content.n_cases)
            last_row = self._write_data_rows(ws, content)
            self._apply_styles(ws, content, last_row)

        print(f'  [OK] Sheet "{name}" filled - {content.n_cases} test case(s), {last_row - ROW_DATA_START + 1} rows.')
        return True

    # ── Filled detection ──────────────────────────────────────────────────────

    def _is_filled(self, ws) -> bool:
        """Return True if any 'O' mark exists in data columns (F onward, rows 10+)."""
        for row in ws.iter_rows(min_row=ROW_DATA_START, max_col=60, values_only=True):
            if row and any(cell == 'O' for cell in row[COL_F - 1:]):
                return True
        return False

    def _clear_values_only(self, ws):
        """
        Clear ONLY cell values, never touching font/fill/border/alignment.
        This preserves all the existing template formatting.
        Skips merged-cell slaves (they have no independent value).
        """
        # Collect the top-left cells of all merge ranges (these are writable)
        merged_masters = set()
        for merge_range in ws.merged_cells.ranges:
            merged_masters.add((merge_range.min_row, merge_range.min_col))

        for row in ws.iter_rows():
            for cell in row:
                coord = (cell.row, cell.column)
                # Merged slave cells cannot be written to directly
                if hasattr(cell, 'value'):
                    try:
                        cell.value = None
                    except AttributeError:
                        pass  # MergedCell slave — skip silently


    # ── Metadata rows ─────────────────────────────────────────────────────────

    def _write_metadata(self, ws, content: IcSheetContent):
        # Row 2: Function Code / Function Name
        ws.cell(ROW_FUNC_CODE, COL_A).value = 'Function Code'
        ws.cell(ROW_FUNC_CODE, COL_IC_CODE_VAL).value = content.sheet_name
        ws.cell(ROW_FUNC_CODE, COL_FUNC_NAME_LBL).value = 'Function Name'
        ws.cell(ROW_FUNC_CODE, COL_FUNC_NAME_VAL).value = content.function_name

        # Row 3: Created By / Executed By
        ws.cell(ROW_CREATED_BY, COL_A).value = 'Created By'
        ws.cell(ROW_CREATED_BY, COL_IC_CODE_VAL).value = content.created_by
        ws.cell(ROW_CREATED_BY, COL_EXEC_LBL).value = 'Executed By'
        ws.cell(ROW_CREATED_BY, COL_EXEC_VAL).value = content.executed_by

        # Row 4: Lines of code / Lack of test cases
        ws.cell(ROW_LINES_CODE, COL_A).value = 'Lines  of code'
        if content.lines_of_code:
            ws.cell(ROW_LINES_CODE, COL_IC_CODE_VAL).value = content.lines_of_code
        ws.cell(ROW_LINES_CODE, COL_EXEC_LBL).value = 'Lack of test cases'

        # Row 5: Test requirement
        ws.cell(ROW_TEST_REQ, COL_A).value = 'Test requirement'
        ws.cell(ROW_TEST_REQ, COL_IC_CODE_VAL).value = (
            '<Brief description about requirements which are tested in this function>'
        )

    def _write_stats(self, ws, content: IcSheetContent):
        # Row 6: headers
        ws.cell(ROW_STATS_HDR, COL_A).value = 'Passed'
        ws.cell(ROW_STATS_HDR, 3).value = 'Failed'
        ws.cell(ROW_STATS_HDR, COL_F).value = 'Untested'
        ws.cell(ROW_STATS_HDR, 12).value = 'N/A/B'
        ws.cell(ROW_STATS_HDR, COL_TOTAL_TC).value = 'Total Test Cases'

        # Row 7: values
        ws.cell(ROW_STATS, COL_A).value = content.passed_count
        ws.cell(ROW_STATS, 3).value = 0
        ws.cell(ROW_STATS, COL_F).value = 0
        ws.cell(ROW_STATS, COL_TOTAL_TC).value = content.n_cases

    # ── UTCID header row ──────────────────────────────────────────────────────

    def _write_utcid_headers(self, ws, n_cases: int):
        for i in range(1, n_cases + 1):
            ws.cell(ROW_UTCID, COL_F + i - 1).value = f'UTCID{i:02d}'

    # ── Data rows ─────────────────────────────────────────────────────────────

    def _write_data_rows(self, ws, content: IcSheetContent) -> int:
        """Write all IcRow objects. Returns the last row number written."""
        cur = ROW_DATA_START
        for ic_row in content.rows:
            self._write_row(ws, cur, ic_row, content)
            cur += 1
        return cur - 1

    def _write_row(self, ws, row: int, ic_row: IcRow, content: IcSheetContent):
        # Col A - section label (only if set, i.e. first row of section)
        if ic_row.section_label:
            ws.cell(row, COL_A).value = ic_row.section_label

        # Col B - sub label
        if ic_row.sub_label:
            ws.cell(row, COL_B).value = ic_row.sub_label

        # Col D - value (omit for header rows)
        if ic_row.value and not ic_row.is_header:
            ws.cell(row, COL_D).value = ic_row.value

        # UTCID columns
        for utcid_idx, mark_val in ic_row.marks.items():
            col = COL_F + utcid_idx - 1
            if mark_val == '__DATE__':
                ws.cell(row, col).value = content.executed_date
            else:
                ws.cell(row, col).value = mark_val

    # ── Styling ───────────────────────────────────────────────────────────────

    def _apply_styles(self, ws, content: IcSheetContent, last_data_row: int):
        n = content.n_cases
        max_col = COL_F + n - 1

        # ── Column widths ──────────────────────────────────────────────────
        ws.column_dimensions['A'].width = 12
        ws.column_dimensions['B'].width = 20
        ws.column_dimensions['C'].width = 6
        ws.column_dimensions['D'].width = 50
        ws.column_dimensions['E'].width = 4

        for i in range(1, n + 1):
            ws.column_dimensions[get_column_letter(COL_F + i - 1)].width = 14

        # ── Cell styles for data area ──────────────────────────────────────
        center_align = Alignment(horizontal='center', vertical='center', wrap_text=True)
        left_align = Alignment(vertical='center', wrap_text=True)
        bold_font = Font(bold=True)

        for r in range(ROW_DATA_START, last_data_row + 1):
            for c in range(1, max_col + 1):
                cell = ws.cell(r, c)
                if c >= COL_F:
                    cell.alignment = center_align
                elif c == COL_D:
                    cell.alignment = left_align
                else:
                    cell.alignment = left_align
                # Bold section / sub-label cells
                if c in (COL_A, COL_B) and cell.value:
                    cell.font = bold_font

        # ── UTCID header row styles ────────────────────────────────────────
        for i in range(1, n + 1):
            cell = ws.cell(ROW_UTCID, COL_F + i - 1)
            cell.alignment = center_align
            cell.font = Font(bold=True)

        # ── Metadata bold labels ───────────────────────────────────────────
        for r in (ROW_FUNC_CODE, ROW_CREATED_BY, ROW_LINES_CODE, ROW_TEST_REQ,
                  ROW_STATS_HDR):
            ws.cell(r, COL_A).font = bold_font
            ws.cell(r, COL_FUNC_NAME_LBL).font = bold_font

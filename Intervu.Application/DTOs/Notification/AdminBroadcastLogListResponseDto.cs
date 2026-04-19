namespace Intervu.Application.DTOs.Notification
{
    public class AdminBroadcastLogListResponseDto
    {
        public List<AdminBroadcastLogDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}

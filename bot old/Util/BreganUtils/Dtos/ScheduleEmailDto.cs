namespace BreganUtils.Dtos
{
    public class ScheduleEmailDto
    {
        public string ToEmailAddress { get; set; }
        public string ToEmailAddressName { get; set; }
        public string FromEmailAddress { get; set; }
        public string FromEmailAddressName { get; set; }
        public object Content { get; set; }
        public string TemplateId { get; set; }
    }
}
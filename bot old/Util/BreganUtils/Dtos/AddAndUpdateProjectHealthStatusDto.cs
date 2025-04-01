namespace BreganUtils.Dtos
{
    public class AddAndUpdateProjectHealthStatusDto
    {
        public string ProjectName { get; set; }
        public TimeSpan ProjectUptime { get; set; }
        public double CPUUsage { get; set; }
        public long RAMUsage { get; set; }
    }
}
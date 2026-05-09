using System;

namespace ZeroPassAlert.Models
{
    public class VisitorEventVO
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string VisitLoc { get; set; }
        public string VisitPurpose { get; set; }
        public long Count { get; set; }
        public DateTime VisitDt { get; set; }
    }

}

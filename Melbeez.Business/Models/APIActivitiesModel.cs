using System;

namespace Melbeez.Business.Models
{
    public class APIActivitiesResponseModel
    {
        public long Id { get; set; }
        public string APIPath { get; set; }
        public double AvarageExecutionTime { get; set;}
        public int TotalNumberOfCall { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class APIActivitiesRequestModel
    {
        public string APIPath { get; set; }
        public int ExecutionTime { get; set; }
    }
}

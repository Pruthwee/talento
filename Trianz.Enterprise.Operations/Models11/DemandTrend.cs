using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Trianz.Enterprise.Operations.General;

namespace Trianz.Enterprise.Operations.Models
{
    public class DemandData
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Practice { get; set; }
        public string HRRFNumber { get; set; }
        public string JobDescription { get; set; }
        public string SubmittedDate { get; set; }


        public static List<DemandData> GetData()
        {
            // Check Redis distributed cache first (replaces static collection)
            var cacheKey = "TrianzOperations:DemandTrend:GetData";
            var cachedJson = RedisCacheHelper.StringGet(cacheKey);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                return JsonConvert.DeserializeObject<List<DemandData>>(cachedJson);
            }

            var data = new List<DemandData>();

            data.Add(new DemandData("JAN", 10,9));
            data.Add(new DemandData("FEB", 15,10));
            data.Add(new DemandData("MAR", 18,14));
            data.Add(new DemandData("APR", 20,15));
            data.Add(new DemandData("MAY", 22,18));
            data.Add(new DemandData("JUNE", 25,10));
            data.Add(new DemandData("JUL", 30,15));
            data.Add(new DemandData("AUG", 33,18));
            data.Add(new DemandData("SEP", 15,14));
            data.Add(new DemandData("OCT", 40,29));
            data.Add(new DemandData("NOV", 32,22));
            data.Add(new DemandData("DEC", 25,27));

            // Cache in Redis for shared access across instances
            var json = JsonConvert.SerializeObject(data);
            RedisCacheHelper.StringSet(cacheKey, json, TimeSpan.FromMinutes(15));

            return data;
        }

        

        public static List<DemandData> GetPieChartData(int TotalDemands, int InternalDemands, int ExternalContractDemands, int ExternalContracttoHireDemands, int ExternalRecruitDemands)
        {
            // Check Redis distributed cache first (replaces static collection)
            var cacheKey = $"TrianzOperations:DemandTrend:GetPieChartData:{TotalDemands}:{InternalDemands}:{ExternalContractDemands}:{ExternalContracttoHireDemands}:{ExternalRecruitDemands}";
            var cachedJson = RedisCacheHelper.StringGet(cacheKey);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                return JsonConvert.DeserializeObject<List<DemandData>>(cachedJson);
            }

            var data = new List<DemandData>();

            data.Add(new DemandData("Total No Of Demands", TotalDemands));
            data.Add(new DemandData("Internal Fullfilled", InternalDemands));
            data.Add(new DemandData("External Fullfilled", ExternalContractDemands));
            data.Add(new DemandData("External-Contract to Hire", ExternalContracttoHireDemands));
            data.Add(new DemandData("External-Recruit", ExternalRecruitDemands));

            // Cache in Redis for shared access across instances
            var json = JsonConvert.SerializeObject(data);
            RedisCacheHelper.StringSet(cacheKey, json, TimeSpan.FromMinutes(15));

            return data;
        }

        public DemandData(string label, double value1)
        {
            this.Label = label;
            this.Value1 = value1;
            
        }
        public DemandData(double value2)
        {
            this.Value2 = value2;
        }

        public DemandData(string label, double value1, double value2)
        {
            this.Label = label;
            this.Value1 = value1;
            this.Value2 = value2;
        }
        public DemandData(string label, double value1, double value2, double value3, double value4)
        {
            this.Label = label;
            this.Value1 = value1;
            this.Value2 = value2;
            this.Value3= value3;
            this.Value4 = value4;
            
        }
        
        public class monthdemandtest
        {
            public string Month { get; set; }
            public int FullFilledRequestCount { get; set; }
            public int SubmittedRequestCount { get; set; }
            public int QualifiedRequestCount { get; set; }
            public int DraftRequestCount { get; set; }
        }
        public class RecordList
        {
            public string Month { get; set; }
            public int count { get; set; }
            public string FullFilled { get; set; }
            public string Submitted { get; set; }
            public string Draft { get; set; }
            public string Qualified { get; set; }
        }

        internal static object GetLineAreaChartDataMonth(List<monthdemandtest> monthlyDemand)
        {
            // Check Redis distributed cache first (replaces static collection)
            var cacheKey = "TrianzOperations:DemandTrend:GetLineAreaChartDataMonth";
            var cachedJson = RedisCacheHelper.StringGet(cacheKey);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                return JsonConvert.DeserializeObject<List<DemandData>>(cachedJson);
            }

            var data = new List<DemandData>();
            
            for (int i = 0; i < monthlyDemand.Count; i++)
            {
                data.Add(new DemandData(Convert.ToString(monthlyDemand[i].Month), monthlyDemand[i].SubmittedRequestCount,monthlyDemand[i].DraftRequestCount,monthlyDemand[i].QualifiedRequestCount,monthlyDemand[i].FullFilledRequestCount));
            }

            // Cache in Redis for shared access across instances
            var json = JsonConvert.SerializeObject(data);
            RedisCacheHelper.StringSet(cacheKey, json, TimeSpan.FromMinutes(15));

            return data;
        }
        public string Label { get; set; }
        public double Value1 { get; set; }
        public double Value2 { get; set; }
        public double Value3 { get; set; }
        public double Value4 { get; set; }
        


    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Trianz.Enterprise.Operations.General;
using Trianz.Enterprise.Operations.Models;

namespace Trianz.Enterprise.Operations.ViewModel
{
   
    
    public class ResourceDashboardViewModel
    {
       
       
  

        public ResourceDashboardViewModel(string label, double value1)
        {
            this.Label = label;
            this.Value1 = value1;
        }
        public ResourceDashboardViewModel(double value2)
        {
            this.Value2 = value2;
        }

        internal static object GetLineAreaChartData(List<IGrouping<string, ProjectAssignment>> ProjectAssignment)
        {
            // Check Redis distributed cache first (replaces in-memory collection)
            var cacheKey = "TrianzOperations:ResourceDashboardViewModel:GetLineAreaChartData";
            var cachedJson = RedisCacheHelper.StringGet(cacheKey);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                return JsonConvert.DeserializeObject<List<ResourceDashboardViewModel>>(cachedJson);
            }

            var data = new List<ResourceDashboardViewModel>();
            var EmployeeData = new List<ResourceDashboardViewModel>();

            for (int i = 0; i < ProjectAssignment.Count; i++)
            {
                data.Add(new ResourceDashboardViewModel(ProjectAssignment[i].Key, ProjectAssignment[i].ToList().Count));
            }

            // Cache in Redis for shared access across instances
            var json = JsonConvert.SerializeObject(data);
            RedisCacheHelper.StringSet(cacheKey, json, TimeSpan.FromMinutes(15));

            return data;
        }



        public ResourceDashboardViewModel(string label, double value1, double value2)
        {
            this.Label = label;
            this.Value1 = value1;
            this.Value2 = value2;
        }

        public string Label { get; set; }
        public double Value1 { get; set; }
        public double Value2 { get; set; }
    }



}

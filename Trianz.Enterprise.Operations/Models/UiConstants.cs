using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trianz.Enterprise.Operations.General;

namespace Trianz.Enterprise.Operations.Models
{
    public class UiConstants
    {
        static UiConstants()
        {
            // Load service URLs from AWS Systems Manager Parameter Store at startup.
            // Falls back to Web.config AppSettings defaults when Parameter Store is unavailable.
            ParameterStoreConfig.LoadParameters();
        }

        static string strGetLookupService = ParameterStoreConfig.GetAppSetting("GetLookupService") ?? "";
        static string strGetEmployeeService = ParameterStoreConfig.GetAppSetting("GetEmployeeService") ?? "";
        static string strEmployeeURL = ParameterStoreConfig.GetAppSetting("EmployeeURL") ?? "";
        static string strNumberingCliURL = ParameterStoreConfig.GetAppSetting("NumberingCliURL") ?? "";
        static string strHRRFCliUrl = ParameterStoreConfig.GetAppSetting("HRRFCliUrl") ?? "";
        static string strHRRFNumberingCliUrl = ParameterStoreConfig.GetAppSetting("HRRFNumberingCliUrl") ?? "";
        static string strNotificationCliUrl = ParameterStoreConfig.GetAppSetting("NotificationCliUrl") ?? "";
        static string strGetOpportunityByNameorCode = ParameterStoreConfig.GetAppSetting("GetOpportunityByNameorCode") ?? "";



        public static string GetMasterLookupData = "Enterprise/Lookup/{lookuptype}";

        public static string GetLookupService = strGetLookupService;
        public static string GetAllLookUpMethods = "/Enterprise/Lookup";
        public static string GetBasedOnLookupType = "/Enterprise/Lookup/{LOOKUPTYPE}";
        public static string GetApplicationSetting = "/Enterprise/Application/Setting";
        public static string GetApplicationIdSetting = "/Enterprise/Application/{APPLICATION}/Setting";
        public static string GetEmployeeService = strGetEmployeeService;
        public static string GetGetEmployees = "/Enterprise/Employee";
        public static string GetEmployeeByEmail = "/Enterprise/Employee/Email/{EMAILID}";
        public static string GetEmployeeByID = "/Enterprise/Employee/{ID}";
        public static string EmployeeURL = strEmployeeURL;
        public static string NumberingCliURL = strNumberingCliURL;
        public static string HRRFCliUrl = strHRRFCliUrl;
        public static string GetHRRFDetails = "/Enterprise/HRRF";
        public static string GetHRRFByHRRFNumber = "/Enterprise/HRRF/{HRRFNo}";
        public static string InsertHRRFDetails = "/Enterprise/HRRFs";
        public static string UpdateHRRFDetails = "/Enterprise/UpdateHRRF";

        public static string HRRFNumberingCliUrl = strHRRFNumberingCliUrl;
        public static string HRRFNumberingCode = "/Enterprise/NumberingSystem?CompanyCode={CompanyCode}&BranchCode={BranchCode}&TypeCode={TypeCode}";

        public static string NotificationCliUrl = strNotificationCliUrl;
        public static string GetNotifications = "/Enterprise/Notification";
        public static string GetNotificationByID = "/Enterprise/Notification/{id}";
        public static string GetNotificationsPendingForApproval = "/Enterprise/Notification/Approval/{managerid}";
        public static string GetPendingNotificationsByApplication = "Enterprise/Notification/Application/?Manager={managerid}&Application={applicationCode}";

        public static string GetOpportunityByNameorCode = strGetOpportunityByNameorCode;
    }
}
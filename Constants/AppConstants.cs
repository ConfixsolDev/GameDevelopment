namespace TechWebSol.Constants
{
    public class AppConstants
    {
        public static string UserSessionKey = "TechSession";
        public const string Id = "TechWebSol";
        public const string AdminRole = "Admin";

        public static readonly Dictionary<string, string> MailSettings = new() {
            {  "LeaveRquest", "/Leave/Details?id="},
            { "BioData","/PersonalInfoes/Details?id="},
            { "Reimbursement","/Reimbursement/Details?id="},
            { "AppraisalPerformance","/Appraisal/PerformanceForm?id="},
            { "AppraisalPromotion","/Appraisal/PromotabilityForm?id="},
           };
    }

    public static class GenderList
    {
        public static List<String> GetAll()
        {
            List<string> all = new List<string>();
            all.Add("Male");
            all.Add("Female");
            return all;
        }
    }

    public static class ServiceTypeList
    {
        public static List<String> GetAll()
        {
            List<string> all = new List<string>();
            all.Add("Contract");
            all.Add("Permanent");
            all.Add("COS");
            all.Add("Internee");
            all.Add("Daily Wages");
            all.Add("Regular Civilian");
            all.Add("Ex-Service Contract");
            all.Add("Ex-Service Regular");
            return all;
        }
    }

    public static class EmploymentCategory
    {
        public static List<String> GetAll()
        {
            List<string> all = new List<string>();
            all.Add("Managment");
            all.Add("Staff");
            all.Add("COS");
            return all;
        }
    }

    public static class AddressTypesList
    {
        public static List<String> GetAll()
        {
            List<string> all = new List<string>();
            all.Add("Present");
            all.Add("Permanent");
            all.Add("In-Laws");
            all.Add("Job");
            return all;
        }
    }

    public static class Relationships
    {
        public static List<String> GetAll()
        {
            List<string> all = new List<string>();
            all.Add("Father");
            all.Add("Mother");
            all.Add("Brother");
            all.Add("Sister");
            all.Add("Son");
            all.Add("Daughter");
            all.Add("Wife");
            all.Add("Husband");

            all.Add("Step Father");
            all.Add("Step Mother");
            all.Add("Step Brother");
            all.Add("Step Sister");
            all.Add("Step Son");
            all.Add("Step Daughter");
            return all;
        }
    }

    public static class SonDaughter
    {
        public static List<String> GetAll()
        {
            List<string> all = new List<string>();
            all.Add("Son");
            all.Add("Daughter");
            all.Add("Step Son");
            all.Add("Step Daughter");
            return all;
        }
    }

    public static class NomineeList
    {
        public static List<String> GetAll()
        {
            List<string> all = new List<string>();
            all.Add("Next of Kin");
            all.Add("Group Insurance Nominee");
            all.Add("Provident Fund Nominee");
            return all;
        }
    }

    public static class HusbandWife
    {
        public static List<String> GetAll()
        {
            List<string> all = new List<string>();
            all.Add("Husband");
            all.Add("Wife");
            all.Add("Ex-Husband");
            all.Add("Ex-Wife");
            all.Add("2nd Wife");
            all.Add("3rd Wife");
            all.Add("4th Wife");
            return all;
        }
    }

    public static class WorkFlowNameList
    {
        public static List<String> GetAll()
        {
            List<string> all = new List<string>();

            all.Add("Info Shared");
            all.Add("Initiated By");
            all.Add("End User");
            all.Add("Recommended By");
            all.Add("Reviewed By");
            all.Add("Approved By");

            return all;
        }
    }
    public static class TicketStatusList
    {
        public static List<String> GetAll()
        {
            List<string> all = new List<string>();

            all.Add("New");
            all.Add("Reopened");
            all.Add("On Hold");
            all.Add("Closed");
            all.Add("In Progress");
            all.Add("Cancelled");

            return all;
        }
    }
    public static class MaritalStatusList
    {
        public static List<String> GetAll()
        {
            List<string> all = new List<string>();

            all.Add("Single");
            all.Add("Married");
            all.Add("Remarried");
            all.Add("Separated");
            all.Add("Divorced");
            all.Add("Widowed");

            return all;
        }
    }
}

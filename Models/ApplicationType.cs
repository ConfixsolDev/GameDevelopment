using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TechWebSol.ViewModels;

namespace TechWebSol.Models
{
    public class Country : LovEntity
    {

        [DisplayName("Country")]
        public override string Name { get; set; }

    }
    public class Province : LovEntity
    {

        [DisplayName("Province")]
        public override string Name { get; set; }
        public virtual Country Country { get; set; }
        public int? CountryId { get; set; }
    }

    public class District : LovEntity
    {
        public virtual Province Province { get; set; }
        public int? ProvinceId { get; set; }

        [DisplayName("District")]
        public override string Name { get; set; }
    }

    public class Sect : LovEntity
    {
        [DisplayName("Sect")]
        public override string Name { get; set; }
    }
    public class Caste : LovEntity
    {
        [DisplayName("Caste")]
        public override string Name { get; set; }
    }

    public class Religion : LovEntity
    {
        [DisplayName("Religion")]
        public override string Name { get; set; }
    }

    public class Designation : LovEntity
    {

        [DisplayName("Designation")]
        public override string Name { get; set; }
        public bool IsOfficer { get; set; } = false;
    }

    public class Department : LovEntity
    {
        [DisplayName("Department")]
        public override string Name { get; set; }
    }

    public class DegreeType : LovEntity
    {

        [DisplayName("DegreeType")]
        public override string Name { get; set; }

        [Range(1, 21, ErrorMessage = "Year of Education must be between 1 and 21.")]
        public int YearOfEducation { get; set; } = 1;

    }
    public class Relation : LovEntity
    {

        [DisplayName("Relation")]
        public override string Name { get; set; }
    }
    public class BloodGroup : LovEntity
    {

        [DisplayName("BloodGroup")]
        public override string Name { get; set; }
    }

    public class PostAppliedFor : LovEntity
    {
        [DisplayName("Post Applied For")]
        public override string Name { get; set; }
    }

    public class Language : LovEntity
    {
        [DisplayName("Language")]
        public override string Name { get; set; }
    }
}

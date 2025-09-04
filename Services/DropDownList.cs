using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechWebSol.Constants;
using TechWebSol.Data;
using TechWebSol.Models.PersonalModals;
using System.ComponentModel.DataAnnotations;

namespace TechWebSol.Services
{
    public interface IGetSetupTypeLists
    {
        IEnumerable<SelectListItem> GetGenderSelectList();
        IEnumerable<SelectListItem> GetRelationshipList();
        IEnumerable<SelectListItem> GetWorkFlowStepNameSelectList();
        IEnumerable<SelectListItem> GetMaritalStatusSelectList();

        IEnumerable<SelectListItem> GetReligionSelectList();
        IEnumerable<SelectListItem> GetReligionNameSelectList();
        IEnumerable<SelectListItem> GetBloodGroupSelectList();
        IEnumerable<SelectListItem> GetDegreeTypeSelectList();
        IEnumerable<SelectListItem> GetSectSelectList();
        IEnumerable<SelectListItem> GetSectNameSelectList();
        IEnumerable<SelectListItem> GetCasteSelectList();
        IEnumerable<SelectListItem> GetCasteNameSelectList();

        //Address dropdown
        IEnumerable<SelectListItem> GetCountrySelectList();
        IEnumerable<SelectListItem> GetCountryNameSelectList();
        IEnumerable<SelectListItem> GetCountryList();
        IEnumerable<SelectListItem> GetProvinceList();
        IEnumerable<SelectListItem> GetProvinceSelectList();
        IEnumerable<SelectListItem> GetDistrictSelectList();
        IEnumerable<SelectListItem> GetDistrictNameSelectList();
        IEnumerable<SelectListItem> GetAddressTypesList();

        IEnumerable<SelectListItem> GetPostAppliedForSelectList();
        IEnumerable<SelectListItem> GetLanguagesSelectList();
        IEnumerable<SelectListItem> GetEmploymentStatusSelectList();
    }

    public class GetSetupTypeLists : IGetSetupTypeLists
    {

        private readonly ApplicationDbContext _context;
        public GetSetupTypeLists(
            ApplicationDbContext context
            )
        {
            _context = context;
        }

        public IEnumerable<SelectListItem> GetRelationList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Relation.AsNoTracking()
                .OrderBy(x => x.Id).Where(x => x.IsDeleted == false)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetPostAppliedForSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.PostAppliedFor.AsNoTracking()
                .Where(x => x.IsDeleted == false)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }
      

        public IEnumerable<SelectListItem> GetServiceTypeSelectList()
        {
            return new SelectList(ServiceTypeList.GetAll());
        }

        public IEnumerable<SelectListItem> GetAddressTypesList()
        {
            return new SelectList(AddressTypesList.GetAll());
        }

        public IEnumerable<SelectListItem> GetGenderSelectList()
        {
            return new SelectList(GenderList.GetAll());
        }

        public IEnumerable<SelectListItem> GetRelationshipList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Relation.AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Name.ToString(),
                    Text = x.Name
                }).ToList();
            return list;
        }
        public IEnumerable<SelectListItem> GetSonDaughterList()
        {
            return new SelectList(SonDaughter.GetAll());
        }
        public IEnumerable<SelectListItem> GetNomineeList()
        {
            return new SelectList(NomineeList.GetAll());
        }
        public IEnumerable<SelectListItem> GetHusbandWifeList()
        {
            return new SelectList(HusbandWife.GetAll());
        }

        public IEnumerable<SelectListItem> GetWorkFlowStepNameSelectList()
        {
            return new SelectList(WorkFlowNameList.GetAll());
        }

        public IEnumerable<SelectListItem> GetMaritalStatusSelectList()
        {
            return new SelectList(MaritalStatusList.GetAll());
        }


        public IEnumerable<SelectListItem> GetProvinceSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Province.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetDistrictSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.District.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetDistrictNameSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.District.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Name,
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }
        public IEnumerable<SelectListItem> GetReligionSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Religion.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }
        public IEnumerable<SelectListItem> GetReligionNameSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Religion.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Name,
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetBloodGroupSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.BloodGroup.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }
        public IEnumerable<SelectListItem> GetDegreeTypeSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.DegreeType.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }
        public IEnumerable<SelectListItem> GetCountrySelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Country.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetCountryNameSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Country.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Name,
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }
        public IEnumerable<SelectListItem> GetCountryList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Country.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetProvinceList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Province.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }
        public IEnumerable<SelectListItem> GetSectSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Sect.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }
        public IEnumerable<SelectListItem> GetSectNameSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Sect.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Name,
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetCasteSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Caste.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetCasteNameSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Caste.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).AsNoTracking()
                //.OrderBy(x => x.Id)
                .Select(x => new SelectListItem
                {
                    Value = x.Name,
                    Text = x.Name
                }).ToList();
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetLanguagesSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Language.AsNoTracking()
                .Where(x => x.IsDeleted == false)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Name,
                    Text = x.Name
                }).ToList();
            return new SelectList(list, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetEmploymentStatusSelectList()
        {
            var statusList = new List<SelectListItem>();
            foreach (EmploymentStatus status in Enum.GetValues<EmploymentStatus>())
            {
                var displayName = status.GetType()
                    .GetField(status.ToString())
                    ?.GetCustomAttributes(typeof(DisplayAttribute), false)
                    .FirstOrDefault() as DisplayAttribute;
                
                statusList.Add(new SelectListItem
                {
                    Value = ((int)status).ToString(),
                    Text = displayName?.Name ?? status.ToString()
                });
            }
            
            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            statusList.Insert(0, blankOption);
            return new SelectList(statusList, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetDistrictSelectListByProvince(int Id)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.District.Where(x => x.IsDeleted == false).OrderBy(x => x.Name).Where(x => x.ProvinceId == Id).AsNoTracking()
                            //.OrderBy(x => x.Id)
                            .Select(x => new SelectListItem
                            {
                                Value = x.Name,
                                Text = x.Name
                            }).ToList();

            SelectListItem blankOption = new SelectListItem()
            {
                Disabled = true,
                Selected = true,
                Value = "",
                Text = "Select"
            };
            list.Insert(0, blankOption);
            return new SelectList(list, "Value", "Text");
        }
    }
}

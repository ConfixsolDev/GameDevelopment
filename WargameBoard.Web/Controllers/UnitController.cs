using Microsoft.AspNetCore.Mvc;
using WargameBoard.Web.Models.ViewModels;
using WargameBoard.Web.Services;
using WargameBoard.Core.Entities;
using AutoMapper;

namespace WargameBoard.Web.Controllers
{
    public class UnitController : Controller
    {
        private readonly ISelectListService _selectListService;
        private readonly IMapper _mapper;

        public UnitController(ISelectListService selectListService, IMapper mapper)
        {
            _selectListService = selectListService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new UnitEditVm();
            await PopulateSelectListsAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UnitEditVm model)
        {
            if (ModelState.IsValid)
            {
                // Map ViewModel to Entity
                var unit = _mapper.Map<Unit>(model);

                // Here you would save to database
                // await _unitRepository.AddAsync(unit);

                return RedirectToAction(nameof(Index));
            }

            await PopulateSelectListsAsync(model);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            // Here you would get the unit from database
            // var unit = await _unitRepository.GetByIdAsync(id);

            // For demo purposes, create a sample unit
            var unit = new Unit
            {
                Id = id,
                Name = "Sample Unit",
                SideId = 1,
                UnitTypeId = 1,
                Personnel = 100,
                VehiclesPrimary = 10,
                Quality = QualityLevel.Veteran,
                Cohesion = 8,
                MovementProfileId = 1
            };

            var viewModel = _mapper.Map<UnitEditVm>(unit);
            await PopulateSelectListsAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UnitEditVm model)
        {
            if (ModelState.IsValid)
            {
                // Map ViewModel to Entity
                var unit = _mapper.Map<Unit>(model);

                // Here you would update in database
                // await _unitRepository.UpdateAsync(unit);

                return RedirectToAction(nameof(Index));
            }

            await PopulateSelectListsAsync(model);
            return View(model);
        }

        private async Task PopulateSelectListsAsync(UnitEditVm model)
        {
            model.Sides = await _selectListService.GetSidesAsync();
            model.UnitTypes = await _selectListService.GetUnitTypesAsync();
            model.MovementProfiles = await _selectListService.GetMovementProfilesAsync();
        }
    }
}

﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    public class MenuItemSelectorViewModel : ObservableObject
    {
        private ScreenMenu _currentScreenMenu;
        private readonly DelegateCommand<ScreenMenuItemData> _addMenuItemCommand;

        public ObservableCollection<ScreenMenuItemButton> MostUsedMenuItems { get; set; }
        public ObservableCollection<ScreenCategoryButton> Categories { get; set; }
        public ObservableCollection<ScreenMenuItemButton> MenuItems { get; set; }
        public DelegateCommand<ScreenMenuCategory> CategoryCommand { get; set; }
        public DelegateCommand<ScreenMenuItem> MenuItemCommand { get; set; }
        public DelegateCommand<string> TypeValueCommand { get; set; }
        public DelegateCommand<string> FindTableCommand { get; set; }
        public DelegateCommand<string> FindMenuItemCommand { get; set; }
        public DelegateCommand<string> FindTicketCommand { get; set; }
        public ICaptionCommand IncPageNumberCommand { get; set; }
        public ICaptionCommand DecPageNumberCommand { get; set; }

        public ScreenMenuCategory MostUsedItemsCategory { get; set; }

        private ScreenMenuCategory _selectedCategory;
        public ScreenMenuCategory SelectedCategory { get { return _selectedCategory; } set { _selectedCategory = value; RaisePropertyChanged("SelectedCategory"); } }

        public string NumeratorValue
        {
            get { return AppServices.MainDataContext.NumeratorValue; }
            set
            {
                AppServices.MainDataContext.NumeratorValue = value;
                RaisePropertyChanged("NumeratorValue");
            }
        }

        public string[] QuickNumeratorValues { get; set; }
        public string[] AlphaButtonValues { get; set; }

        public bool IsQuickNumeratorVisible { get { return SelectedCategory != null && SelectedCategory.IsQuickNumeratorVisible; } }
        public bool IsNumeratorVisible { get { return SelectedCategory != null && SelectedCategory.IsNumeratorVisible; } }
        public bool IsPageNumberNavigatorVisible { get { return SelectedCategory != null && SelectedCategory.PageCount > 1; } }
        public VerticalAlignment MenuItemsVerticalAlignment { get { return SelectedCategory != null && SelectedCategory.ButtonHeight > 0 ? VerticalAlignment.Top : VerticalAlignment.Stretch; } }
        public VerticalAlignment CategoriesVerticalAlignment { get { return Categories.Count > 0 && double.IsNaN(Categories[0].MButtonHeight) ? VerticalAlignment.Stretch : VerticalAlignment.Top; } }
        public int CurrentPageNo { get; set; }

        public MenuItemSelectorViewModel(DelegateCommand<ScreenMenuItemData> addMenuItemCommand)
        {
            _addMenuItemCommand = addMenuItemCommand;
            CategoryCommand = new DelegateCommand<ScreenMenuCategory>(OnCategoryCommandExecute);
            MenuItemCommand = new DelegateCommand<ScreenMenuItem>(OnMenuItemCommandExecute);
            TypeValueCommand = new DelegateCommand<string>(OnTypeValueExecute);
            FindTableCommand = new DelegateCommand<string>(OnFindTableExecute, CanFindTable);
            FindMenuItemCommand = new DelegateCommand<string>(OnFindMenuItemCommand);
            FindTicketCommand = new DelegateCommand<string>(OnFindTicketExecute, CanFindTicket);
            IncPageNumberCommand = new CaptionCommand<string>("Sonraki Sayfa >>", OnIncPageNumber, CanIncPageNumber);
            DecPageNumberCommand = new CaptionCommand<string>("<< Önceki Sayfa", OnDecPageNumber, CanDecPageNumber);


            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(OnDepartmentChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnNumeratorReset);
            NumeratorValue = "";

        }

        private void OnNumeratorReset(EventParameters<EventAggregator> obj)
        {
            if (obj.Topic == EventTopicNames.ResetNumerator)
                NumeratorValue = "";
        }

        private void OnDecPageNumber(string obj)
        {
            CurrentPageNo--;
            UpdateMenuButtons(SelectedCategory);
        }

        private bool CanDecPageNumber(string arg)
        {
            return CurrentPageNo > 0;
        }

        private bool CanIncPageNumber(object arg)
        {
            return SelectedCategory != null && CurrentPageNo < SelectedCategory.PageCount - 1;
        }

        private void OnIncPageNumber(object obj)
        {
            CurrentPageNo++;
            UpdateMenuButtons(SelectedCategory);
        }

        private void OnFindTicketExecute(string obj)
        {
            if (string.IsNullOrEmpty(NumeratorValue))
            {
                AppServices.MainDataContext.CurrentWorkPeriod.PublishEvent(EventTopicNames.DisplayTicketExplorer);
            }
            else
            {
                AppServices.MainDataContext.OpenTicketFromTicketNumber(NumeratorValue);

                if (AppServices.MainDataContext.SelectedTicket != null)
                {
                    if (!AppServices.IsUserPermittedFor(PermissionNames.DisplayOldTickets) && AppServices.MainDataContext.SelectedTicket.Date < AppServices.MainDataContext.CurrentWorkPeriod.StartDate)
                    {
                        AppServices.MainDataContext.CloseTicket();
                    }
                    else
                    {
                        AppServices.MainDataContext.SelectedTicket.PublishEvent(EventTopicNames.TicketSelectedFromTableList);
                    }
                }
                NumeratorValue = "";
            }
        }

        private static bool CanFindTicket(string arg)
        {
            return AppServices.MainDataContext.SelectedTicket == null;
        }

        private void OnFindMenuItemCommand(string obj)
        {
            string insertedData = NumeratorValue;
            decimal quantity = 1;
            if (NumeratorValue.ToLower().Contains("x"))
            {
                insertedData = NumeratorValue.Substring(NumeratorValue.ToLower().IndexOf("x") + 1);
                string q = NumeratorValue.Substring(0, NumeratorValue.ToLower().IndexOf("x"));
                decimal.TryParse(q, out quantity);
            }
            NumeratorValue = "";

            if (quantity > 0)
            {
                var weightBarcodePrefix = AppServices.SettingService.WeightBarcodePrefix;
                if (!string.IsNullOrEmpty(weightBarcodePrefix) && insertedData.StartsWith(weightBarcodePrefix))
                {
                    var itemLength = AppServices.SettingService.WeightBarcodeItemLength;
                    var quantityLength = AppServices.SettingService.WeightBarcodeQuantityLength;
                    if (itemLength > 0 && quantityLength > 0)
                    {
                        var bc = insertedData.Substring(weightBarcodePrefix.Length, itemLength);
                        var qty = insertedData.Substring(weightBarcodePrefix.Length + itemLength, quantityLength);
                        if (bc.Length > 0 && qty.Length > 0)
                        {
                            insertedData = bc;
                            decimal.TryParse(qty, out quantity);
                        }
                    }
                }
                var mi = AppServices.DataAccessService.GetMenuItem(insertedData);
                if (mi != null)
                {
                    var si = new ScreenMenuItem { MenuItemId = mi.Id, Name = mi.Name };
                    var data = new ScreenMenuItemData { ScreenMenuItem = si, Quantity = quantity };
                    _addMenuItemCommand.Execute(data);
                }
            }
        }

        private static bool CanFindTable(string arg)
        {
            return AppServices.MainDataContext.SelectedTicket == null;
        }

        private void OnFindTableExecute(string obj)
        {
            if (AppServices.MainDataContext.SelectedTicket == null)
            {
                AppServices.MainDataContext.OpenTicketFromTableName(NumeratorValue);
                if (AppServices.MainDataContext.SelectedTicket != null)
                    AppServices.MainDataContext.SelectedTicket.PublishEvent(EventTopicNames.TicketSelectedFromTableList);
            }
            NumeratorValue = "";
        }

        private void OnMenuItemCommandExecute(ScreenMenuItem screenMenuItem)
        {
            decimal selectedMultiplier = 1;
            if (!string.IsNullOrEmpty(NumeratorValue))
                decimal.TryParse(NumeratorValue, out selectedMultiplier);

            if (IsQuickNumeratorVisible)
                NumeratorValue = QuickNumeratorValues[0];
            if (IsNumeratorVisible)
                NumeratorValue = "";

            if (selectedMultiplier > 0)
            {
                var data = new ScreenMenuItemData { ScreenMenuItem = screenMenuItem, Quantity = selectedMultiplier };
                if (data.Quantity == 1 && screenMenuItem.Quantity > 1)
                    data.Quantity = screenMenuItem.Quantity;
                _addMenuItemCommand.Execute(data);
            }
        }

        private void OnDepartmentChanged(EventParameters<Department> department)
        {
            if (department.Topic == EventTopicNames.SelectedDepartmentChanged)
            {
                _currentScreenMenu = department.Value != null
                    ? AppServices.DataAccessService.GetScreenMenu(department.Value.ScreenMenuId)
                    : null;

                Categories = CreateCategoryButtons(_currentScreenMenu);
                MostUsedItemsCategory = null;
                MostUsedMenuItems = CreateMostUsedMenuItems(_currentScreenMenu);

                if (Categories != null && Categories.Count == 1)
                {
                    OnCategoryCommandExecute(Categories[0].Category);
                    Categories.Clear();
                }
                RaisePropertyChanged("Categories");
                RaisePropertyChanged("CategoriesVerticalAlignment");
                RaisePropertyChanged("MostUsedMenuItems");
                RaisePropertyChanged("MostUsedItemsCategory");
            }
        }

        private ObservableCollection<ScreenMenuItemButton> CreateMostUsedMenuItems(ScreenMenu screenMenu)
        {
            if (screenMenu != null)
            {
                MostUsedItemsCategory = screenMenu.Categories.FirstOrDefault(x => x.MostUsedItemsCategory);
                if (MostUsedItemsCategory != null)
                {
                    return new ObservableCollection<ScreenMenuItemButton>(
                    MostUsedItemsCategory.ScreenMenuItems.OrderBy(x => x.Order).Select(x => new ScreenMenuItemButton(x, MenuItemCommand, MostUsedItemsCategory)));
                }
            }
            return null;
        }

        private void UpdateMenuButtons(ScreenMenuCategory category)
        {
            MenuItems = CreateMenuButtons(category, CurrentPageNo);
            RaisePropertyChanged("MenuItems");
            RaisePropertyChanged("IsPageNumberNavigatorVisible");
            RaisePropertyChanged("MenuItemsVerticalAlignment");
        }

        private void OnCategoryCommandExecute(ScreenMenuCategory category)
        {
            CurrentPageNo = 0;
            UpdateMenuButtons(category);
            if (IsQuickNumeratorVisible)
            {
                QuickNumeratorValues = string.IsNullOrEmpty(category.NumeratorValues) ? new[] { "1", "2", "3", "4", "5" } : category.NumeratorValues.Split(',');
                NumeratorValue = QuickNumeratorValues[0];
            }
            else NumeratorValue = "";

            AlphaButtonValues = string.IsNullOrEmpty(category.AlphaButtonValues) ? new string[0] : category.AlphaButtonValues.Split(',');

            RaisePropertyChanged("IsQuickNumeratorVisible");
            RaisePropertyChanged("IsNumeratorVisible");
            RaisePropertyChanged("QuickNumeratorValues");
            RaisePropertyChanged("AlphaButtonValues");
            RaisePropertyChanged("MenuItemsVerticalAlignment");
        }

        private ObservableCollection<ScreenMenuItemButton> CreateMenuButtons(ScreenMenuCategory category, int pageNo)
        {
            var result = new ObservableCollection<ScreenMenuItemButton>();
            SelectedCategory = category;

            var screenMenuItems = AppServices.DataAccessService.GetMenuItems(category, pageNo);

            foreach (var item in screenMenuItems)
            {
                var sButton = new ScreenMenuItemButton(item, MenuItemCommand, category);
                result.Add(sButton);
            }
            return result;
        }

        private ObservableCollection<ScreenCategoryButton> CreateCategoryButtons(ScreenMenu screenMenu)
        {
            if (screenMenu != null)
            {
                if (MenuItems != null) MenuItems.Clear();

                _currentScreenMenu = screenMenu;
                var result = new ObservableCollection<ScreenCategoryButton>();

                foreach (var category in screenMenu.Categories.OrderBy(x => x.Order).Where(x => !x.MostUsedItemsCategory))
                {
                    var sButton = new ScreenCategoryButton(category, CategoryCommand);
                    result.Add(sButton);
                }

                if (result.Count > 0)
                {
                    var c = result.First();
                    if (_selectedCategory != null)
                        c = result.SingleOrDefault(x => x.Category.Name.ToLower() == _selectedCategory.Name.ToLower());
                    if (c == null && result.Count > 0) c = result.ElementAt(0);
                    if (c != null) OnCategoryCommandExecute(c.Category);
                }

                return result;
            }

            if (MenuItems != null) MenuItems.Clear();
            if (Categories != null) Categories.Clear();
            _currentScreenMenu = null;

            return Categories;
        }

        private void OnTypeValueExecute(string obj)
        {
            if (obj == "\r")
                FindMenuItemCommand.Execute("");
            else if (obj == "\b" && !string.IsNullOrEmpty(NumeratorValue))
                NumeratorValue = NumeratorValue.Substring(0, NumeratorValue.Length - 1);
            else
                NumeratorValue = obj == "c" ? "" : Helpers.AddTypedValue(NumeratorValue, obj, "#0.");
        }

        public bool HandleTextInput(string text)
        {
            if (IsNumeratorVisible)
            {
                OnTypeValueExecute(text);
                return true;
            }
            return false;
        }
    }
}

﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using Samba.Domain.Models.Menus;
using Samba.Localization;
using Samba.Presentation.Common;


namespace Samba.Modules.MenuModule
{
    public enum NumeratorType
    {
        Yok,
        Küçük,
        Büyük
    }

    public class ScreenMenuCategoryViewModel : ObservableObject
    {
        public ScreenMenuCategoryViewModel(ScreenMenuCategory model)
        {
            Model = model;
        }

        [Browsable(false)]
        public ScreenMenuCategory Model { get; private set; }

        [Browsable(false)]
        public string CategoryListDisplay { get { return ScreenMenuItems.Count > 0 ? string.Format("{0} ({1})", Name, ScreenMenuItems.Count) : Name; } }

        [Browsable(false)]
        public IList<ScreenMenuItem> ScreenMenuItems { get { return Model.ScreenMenuItems; } }

        [LocalizedDisplayName(ResourceStrings.CategoryName), LocalizedCategory(ResourceStrings.CategoryProperties)]
        public string Name { get { return Model.Name; } set { Model.Name = value; RaisePropertyChanged("Name"); } }

        [LocalizedDisplayName(ResourceStrings.FastMenu), LocalizedCategory(ResourceStrings.CategoryProperties)]
        public bool MostUsedItemsCategory { get { return Model.MostUsedItemsCategory; } set { Model.MostUsedItemsCategory = value; } }

        [LocalizedDisplayName(ResourceStrings.ButtonHeight), LocalizedCategory(ResourceStrings.CategoryProperties)]
        public int MainButtonHeight { get { return Model.MButtonHeight; } set { Model.MButtonHeight = value; } }

        [LocalizedDisplayName(ResourceStrings.ButtonColor), LocalizedCategory(ResourceStrings.CategoryProperties)]
        public SolidColorBrush MainButtonColor
        {
            get
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(Model.MButtonColor));
            }
            set
            {
                Model.MButtonColor = value != Brushes.Transparent ? value.ToString() : string.Empty;
            }
        }

        [LocalizedDisplayName(ResourceStrings.ImagePath), LocalizedCategory(ResourceStrings.CategoryProperties)]
        public string ImagePath { get { return Model.ImagePath; } set { Model.ImagePath = value; } }

        [LocalizedDisplayName(ResourceStrings.ColumnCount), LocalizedCategory(ResourceStrings.MenuProperties)]
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }

        [LocalizedDisplayName(ResourceStrings.ButtonHeight), LocalizedCategory(ResourceStrings.MenuProperties)]
        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }

        [LocalizedDisplayName(ResourceStrings.PageCount), LocalizedCategory(ResourceStrings.MenuProperties)]
        public int PageCount { get { return Model.PageCount; } set { Model.PageCount= value; } }

        [LocalizedDisplayName(ResourceStrings.WrapText), LocalizedCategory(ResourceStrings.MenuProperties)]
        public bool WrapText { get { return Model.WrapText; } set { Model.WrapText = value; } }

        [LocalizedDisplayName(ResourceStrings.ButtonColor), LocalizedCategory(ResourceStrings.MenuProperties)]
        public SolidColorBrush ButtonColor
        {
            get
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(Model.ButtonColor));
            }
            set
            {
                Model.ButtonColor = value != Brushes.Transparent ? value.ToString() : string.Empty;
            }
        }

        [LocalizedDisplayName(ResourceStrings.NumeratorType), LocalizedCategory(ResourceStrings.NumeratorProperties)]
        public NumeratorType NumeratorType { get { return (NumeratorType)Model.NumeratorType; } set { Model.NumeratorType = (int)value; } }

        [LocalizedDisplayName(ResourceStrings.NumeratorValue), LocalizedCategory(ResourceStrings.NumeratorProperties)]
        public string NumeratorValues { get { return Model.NumeratorValues; } set { Model.NumeratorValues = value; } }

        [LocalizedDisplayName(ResourceStrings.AlphanumericButtonValues), LocalizedCategory(ResourceStrings.NumeratorProperties)]
        public string AlphaButtonValues { get { return Model.AlphaButtonValues; } set { Model.AlphaButtonValues = value; } }

        internal void UpdateDisplay()
        {
            RaisePropertyChanged("CategoryListDisplay");
        }

        public bool ContainsMenuItem(MenuItem item)
        {
            return ScreenMenuItems.Where(x => x.MenuItemId == item.Id).Count() > 0;
        }
    }
}

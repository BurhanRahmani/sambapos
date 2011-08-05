﻿using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tables;
using Samba.Persistance.Data;

namespace Samba.Services
{
    public class DataAccessService
    {
        public IEnumerable<Table> GetCurrentTables(int tableScreenId, int currentPageNo)
        {
            AppServices.MainDataContext.UpdateTables(tableScreenId, currentPageNo);

            var selectedTableScreen = AppServices.MainDataContext.SelectedTableScreen;

            if (selectedTableScreen != null)
            {
                if (selectedTableScreen.PageCount > 1)
                {
                    return selectedTableScreen.Tables
                         .OrderBy(x => x.Order)
                         .Skip(selectedTableScreen.ItemCountPerPage * currentPageNo)
                         .Take(selectedTableScreen.ItemCountPerPage);
                }
                return selectedTableScreen.Tables;
            }
            return new List<Table>();
        }

        public IEnumerable<ScreenMenuItem> GetMenuItems(ScreenMenuCategory category, int currentPageNo)
        {
            if (category.PageCount > 1)
            {
                return category.ScreenMenuItems
                    .OrderBy(x => x.Order)
                    .Skip(category.ItemCountPerPage * currentPageNo)
                    .Take(category.ItemCountPerPage);
            }
            return category.ScreenMenuItems.OrderBy(x => x.Order);
        }

        public ScreenMenu GetScreenMenu(int screenMenuId)
        {
            return Dao.SingleWithCache<ScreenMenu>(x => x.Id == screenMenuId, x => x.Categories,
                                          x => x.Categories.Select(z => z.ScreenMenuItems));
        }

        public MenuItem GetMenuItem(int menuItemId)
        {
            return Dao.SingleWithCache<MenuItem>(x => x.Id == menuItemId, x => x.PropertyGroups.Select(z => z.Properties), x => x.Portions);
        }

        public MenuItem GetMenuItem(string barcode)
        {
            return Dao.SingleWithCache<MenuItem>(x => x.Barcode == barcode, x => x.PropertyGroups.Select(z => z.Properties), x => x.Portions);
        }
        
        public IEnumerable<string> GetInventoryItemNames()
        {
            return Dao.Select<InventoryItem, string>(x => x.Name, x => !string.IsNullOrEmpty(x.Name));
        }

        public Table GetTable(string tableName)
        {
            return Dao.Single<Table>(x => x.Name == tableName);
        }
    }
}

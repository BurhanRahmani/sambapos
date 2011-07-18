﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tables;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    public class DepartmentViewModel : EntityViewModelBase<Department>
    {
        private IWorkspace _workspace;

        private IEnumerable<ScreenMenu> _screenMenus;
        public IEnumerable<ScreenMenu> ScreenMenus
        {
            get { return _screenMenus ?? (_screenMenus = Dao.Query<ScreenMenu>()); }
            set { _screenMenus = value; }
        }

        private IEnumerable<TableScreen> _tableScreens;
        public IEnumerable<TableScreen> TableScreens
        {
            get { return _tableScreens ?? (_tableScreens = Dao.Query<TableScreen>()); }
            set { _tableScreens = value; }
        }

        private ObservableCollection<TicketTagGroupViewModel> _ticketTagGroups;
        public ObservableCollection<TicketTagGroupViewModel> TicketTagGroups
        {
            get { return _ticketTagGroups ?? (_ticketTagGroups = new ObservableCollection<TicketTagGroupViewModel>(GetTicketTags(Model))); }
        }

        private IEnumerable<Numerator> _numerators;
        public IEnumerable<Numerator> Numerators { get { return _numerators ?? (_numerators = _workspace.All<Numerator>()); } set { _numerators = value; } }

        public int ScreenMenuId { get { return Model.ScreenMenuId; } set { Model.ScreenMenuId = value; } }
        public int TerminalScreenMenuId { get { return Model.TerminalScreenMenuId; } set { Model.TerminalScreenMenuId = value; } }

        public Numerator TicketNumerator { get { return Model.TicketNumerator; } set { Model.TicketNumerator = value; } }
        public Numerator OrderNumerator { get { return Model.OrderNumerator; } set { Model.OrderNumerator = value; } }

        public int? TableScreenId
        {
            get { return Model.TableScreenId; }
            set { Model.TableScreenId = value.GetValueOrDefault(0); }
        }

        public int? TerminalTableScreenId
        {
            get { return Model.TerminalTableScreenId; }
            set { Model.TerminalTableScreenId = value.GetValueOrDefault(0); }
        }

        public int OpenTicketViewColumnCount { get { return Model.OpenTicketViewColumnCount; } set { Model.OpenTicketViewColumnCount = value; } }
        public string DefaultTag { get { return Model.DefaultTag; } set { Model.DefaultTag = value; } }
        public string TerminalDefaultTag { get { return Model.TerminalDefaultTag; } set { Model.TerminalDefaultTag = value; } }

        public bool IsFastFood
        {
            get { return Model.IsFastFood; }
            set { Model.IsFastFood = value; }
        }

        public bool IsAlaCarte
        {
            get { return Model.IsAlaCarte; }
            set { Model.IsAlaCarte = value; }
        }

        public bool IsTakeAway
        {
            get { return Model.IsTakeAway; }
            set { Model.IsTakeAway = value; }
        }

        public TicketTagGroupViewModel SelectedTicketTag { get; set; }

        public ICaptionCommand AddTicketTagGroupCommand { get; set; }
        public ICaptionCommand DeleteTicketTagGroupCommand { get; set; }

        public DepartmentViewModel(Department model)
            : base(model)
        {
            AddTicketTagGroupCommand = new CaptionCommand<string>("Etiket Grubu Ekle", OnAddTicketTagGroup);
            DeleteTicketTagGroupCommand = new CaptionCommand<string>("Etiket Grubu Sil", OnDeleteTicketTagGroup, CanDeleteTicketTagGroup);
        }

        private bool CanDeleteTicketTagGroup(string arg)
        {
            return SelectedTicketTag != null;
        }

        private void OnDeleteTicketTagGroup(string obj)
        {
            Model.TicketTagGroups.Remove(SelectedTicketTag.Model);
            TicketTagGroups.Remove(SelectedTicketTag);
        }

        private void OnAddTicketTagGroup(string obj)
        {
            var selectedValues =
                InteractionService.UserIntraction.ChooseValuesFrom(_workspace.All<TicketTagGroup>().ToList<IOrderable>(),
                Model.TicketTagGroups.ToList<IOrderable>(), "Adisyon Etiketleri", Model.Name + " departmanında kullanmak istediğiniz etiketleri seçiniz.",
                "Adisyon Etiketleri", "Adisyon Etiketleri");

            foreach (TicketTagGroup selectedValue in selectedValues)
            {
                if (!Model.TicketTagGroups.Contains(selectedValue))
                    Model.TicketTagGroups.Add(selectedValue);
            }

            _ticketTagGroups = new ObservableCollection<TicketTagGroupViewModel>(GetTicketTags(Model));

            RaisePropertyChanged("TicketTagGroups");
        }

        private static IEnumerable<TicketTagGroupViewModel> GetTicketTags(Department model)
        {
            return model.TicketTagGroups.OrderBy(x => x.Order).Select(x => new TicketTagGroupViewModel(x));
        }

        public override Type GetViewType()
        {
            return typeof(DepartmentView);
        }

        public override string GetModelTypeString()
        {
            return "Departman";
        }

        public override void Initialize(IWorkspace workspace)
        {
            _workspace = workspace;
        }
    }
}

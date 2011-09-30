﻿using System;
using System.Linq;
using Axcidv5callerid;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;

namespace Samba.Modules.CidMonitor
{
    [ModuleExport(typeof(CidMonitor))]
    public class CidMonitor : ModuleBase
    {
        public CidMonitor()
        {
            try
            {
                var frmMain = new FrmMain();
                frmMain.axCIDv51.OnCallerID += axCIDv51_OnCallerID;
                frmMain.axCIDv51.Start();
            }
            catch (Exception)
            {
                InteractionService.UserIntraction.DisplayPopup(Resources.Information, Resources.CallerIdDriverError, "", "");
            }
        }

        static void axCIDv51_OnCallerID(object sender, ICIDv5Events_OnCallerIDEvent e)
        {
            var pn = e.phoneNumber;
            pn = pn.TrimStart('+');
            pn = pn.TrimStart('0');
            pn = pn.TrimStart('9');
            pn = pn.TrimStart('0');

            var c = Dao.Query<Account>(x => x.PhoneNumber == pn);
            if (c.Count() == 0)
                c = Dao.Query<Account>(x => x.PhoneNumber.Contains(pn));
            if (c.Count() == 1)
            {
                var account = c.First();
                InteractionService.UserIntraction.DisplayPopup(account.Name, account.Name + " " + Resources.Calling + ".\r" + account.PhoneNumber + "\r" + account.Address + "\r" + account.Note,
                                                            account.PhoneNumber, EventTopicNames.SelectAccount);
            }
            else
                InteractionService.UserIntraction.DisplayPopup(e.phoneNumber, e.phoneNumber + " " + Resources.Calling + "...",
                                                               e.phoneNumber, EventTopicNames.SelectAccount);
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;
using Samba.Domain;
using Samba.Domain.Models.Cashes;
using Samba.Domain.Models.Customers;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.CustomerModule
{
    [Export]
    public class CustomerSelectorViewModel : ObservableObject
    {
        private readonly Timer _updateTimer;

        public ICaptionCommand CloseScreenCommand { get; set; }
        public ICaptionCommand SelectCustomerCommand { get; set; }
        public ICaptionCommand CreateCustomerCommand { get; set; }
        public ICaptionCommand ResetCustomerCommand { get; set; }
        public ICaptionCommand FindTicketCommand { get; set; }
        public ICaptionCommand MakePaymentCommand { get; set; }
        public ICaptionCommand DisplayCustomerAccountCommand { get; set; }
        public ICaptionCommand GetPaymentFromCustomerCommand { get; set; }
        public ICaptionCommand MakePaymentToCustomerCommand { get; set; }
        public ICaptionCommand CloseAccountScreenCommand { get; set; }

        public Ticket SelectedTicket { get { return AppServices.MainDataContext.SelectedTicket; } }
        public ObservableCollection<CustomerViewModel> FoundCustomers { get; set; }

        public ObservableCollection<CustomerTransactionViewModel> SelectedCustomerTransactions { get; set; }

        private int _selectedView;
        public int SelectedView
        {
            get { return _selectedView; }
            set { _selectedView = value; RaisePropertyChanged("SelectedView"); }
        }

        public CustomerViewModel SelectedCustomer { get { return FoundCustomers.Count == 1 ? FoundCustomers[0] : FocusedCustomer; } }

        private CustomerViewModel _focusedCustomer;
        public CustomerViewModel FocusedCustomer
        {
            get { return _focusedCustomer; }
            set
            {
                _focusedCustomer = value;
                RaisePropertyChanged("FocusedCustomer");
                RaisePropertyChanged("SelectedCustomer");
            }
        }

        private string _ticketSearchText;
        public string TicketSearchText
        {
            get { return _ticketSearchText; }
            set { _ticketSearchText = value; RaisePropertyChanged("TicketSearchText"); }
        }

        private string _phoneNumberSearchText;
        public string PhoneNumberSearchText
        {
            get { return string.IsNullOrEmpty(_phoneNumberSearchText) ? null : _phoneNumberSearchText.TrimStart('+', '0', '9'); }
            set
            {
                if (value != _phoneNumberSearchText)
                {
                    _phoneNumberSearchText = value;
                    RaisePropertyChanged("PhoneNumberSearchText");
                    ResetTimer();
                }
            }
        }

        private string _customerNameSearchText;
        public string CustomerNameSearchText
        {
            get { return _customerNameSearchText; }
            set
            {
                if (value != _customerNameSearchText)
                {
                    _customerNameSearchText = value;
                    RaisePropertyChanged("CustomerNameSearchText");
                    ResetTimer();
                }
            }
        }

        private string _addressSearchText;
        public string AddressSearchText
        {
            get { return _addressSearchText; }
            set
            {
                if (value != _addressSearchText)
                {
                    _addressSearchText = value;
                    RaisePropertyChanged("AddressSearchText");
                    ResetTimer();
                }
            }
        }

        public bool IsResetCustomerVisible
        {
            get
            {
                return (AppServices.MainDataContext.SelectedTicket != null &&
                        AppServices.MainDataContext.SelectedTicket.CustomerId > 0);
            }
        }

        public bool IsClearVisible
        {
            get
            {
                return (AppServices.MainDataContext.SelectedTicket != null &&
                        AppServices.MainDataContext.SelectedTicket.CustomerId == 0);
            }
        }

        public bool IsMakePaymentVisible
        {
            get
            {
                return (AppServices.MainDataContext.SelectedTicket != null && AppServices.IsUserPermittedFor(PermissionNames.MakePayment));
            }
        }

        private int _activeView;
        public int ActiveView
        {
            get { return _activeView; }
            set { _activeView = value; RaisePropertyChanged("ActiveView"); }
        }

        public CustomerSelectorViewModel()
        {
            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += UpdateTimerElapsed;
            FoundCustomers = new ObservableCollection<CustomerViewModel>();
            CloseScreenCommand = new CaptionCommand<string>("Kapat", OnCloseScreen);
            SelectCustomerCommand = new CaptionCommand<string>("Müşteri\rSeç", OnSelectCustomer, CanSelectCustomer);
            CreateCustomerCommand = new CaptionCommand<string>("Yeni\rMüşteri", OnCreateCustomer, CanCreateCustomer);
            FindTicketCommand = new CaptionCommand<string>("Adisyon\rBul", OnFindTicket, CanFindTicket);
            ResetCustomerCommand = new CaptionCommand<string>("Müşteri\rİptal", OnResetCustomer, CanResetCustomer);
            MakePaymentCommand = new CaptionCommand<string>("Ödeme\rAl", OnMakePayment, CanMakePayment);
            DisplayCustomerAccountCommand = new CaptionCommand<string>("Hesap\rKartı", OnDisplayCustomerAccount, CanSelectCustomer);
            MakePaymentToCustomerCommand = new CaptionCommand<string>("Ödeme\rYap", OnMakePaymentToCustomerCommand, CanSelectCustomer);
            GetPaymentFromCustomerCommand = new CaptionCommand<string>("Ödeme\rAl", OnGetPaymentFromCustomerCommand, CanSelectCustomer);
            CloseAccountScreenCommand = new CaptionCommand<string>("Kapat", OnCloseAccountScreen);

            SelectedCustomerTransactions = new ObservableCollection<CustomerTransactionViewModel>();
        }

        private void OnCloseAccountScreen(string obj)
        {
            RefreshSelectedCustomer();
        }

        private void OnGetPaymentFromCustomerCommand(string obj)
        {
            SelectedCustomer.Model.PublishEvent(EventTopicNames.GetPaymentFromCustomer);
            FoundCustomers.Clear();
        }

        private void OnMakePaymentToCustomerCommand(string obj)
        {
            SelectedCustomer.Model.PublishEvent(EventTopicNames.MakePaymentToCustomer);
            FoundCustomers.Clear();
        }

        internal void DisplayCustomerAccount(Customer customer)
        {
            FoundCustomers.Clear();
            if (customer != null)
                FoundCustomers.Add(new CustomerViewModel(customer));
            RaisePropertyChanged("SelectedCustomer");
            OnDisplayCustomerAccount("");
        }

        private void OnDisplayCustomerAccount(string obj)
        {
            SaveSelectedCustomer();
            SelectedCustomerTransactions.Clear();
            if (SelectedCustomer != null)
            {
                var tickets = Dao.Query<Ticket>(x => x.CustomerId == SelectedCustomer.Id && x.LastPaymentDate > SelectedCustomer.AccountOpeningDate, x => x.Payments);
                var cashTransactions = Dao.Query<CashTransaction>(x => x.Date > SelectedCustomer.AccountOpeningDate && x.CustomerId == SelectedCustomer.Id);
                var transactions = new List<CustomerTransactionViewModel>();
                transactions.AddRange(tickets.Select(x => new CustomerTransactionViewModel
                                                       {
                                                           Description = x.TicketNumber + " numaralı adisyon",
                                                           Date = x.LastPaymentDate,
                                                           Receivable = x.GetAccountPaymentAmount() + x.GetAccountRemainingAmount(),
                                                           Liability = x.GetAccountPaymentAmount()
                                                       }));

                transactions.AddRange(cashTransactions.Where(x => x.TransactionType == (int)TransactionType.Income)
                    .Select(x => new CustomerTransactionViewModel
                            {
                                Description = x.Name,
                                Date = x.Date,
                                Liability = x.Amount
                            }));

                transactions.AddRange(
                    cashTransactions.Where(x => x.TransactionType == (int)TransactionType.Expense)
                    .Select(x => new CustomerTransactionViewModel
                            {
                                Description = x.Name,
                                Date = x.Date,
                                Receivable = x.Amount
                            }));

                transactions = transactions.OrderBy(x => x.Date).ToList();

                for (var i = 0; i < transactions.Count; i++)
                {
                    transactions[i].Balance = (transactions[i].Receivable - transactions[i].Liability);
                    if (i > 0) (transactions[i].Balance) += (transactions[i - 1].Balance);
                }

                SelectedCustomerTransactions.AddRange(transactions);
            }
            ActiveView = 1;
        }

        private bool CanMakePayment(string arg)
        {
            return SelectedCustomer != null && AppServices.MainDataContext.SelectedTicket != null;
        }

        private void OnMakePayment(string obj)
        {
            SelectedCustomer.Model.PublishEvent(EventTopicNames.PaymentRequestedForTicket);
            ClearSearchValues();
        }

        private bool CanResetCustomer(string arg)
        {
            return AppServices.MainDataContext.SelectedTicket != null &&
                AppServices.MainDataContext.SelectedTicket.CanSubmit &&
                AppServices.MainDataContext.SelectedTicket.CustomerId > 0;
        }

        private void OnResetCustomer(string obj)
        {
            Customer.Null.PublishEvent(EventTopicNames.CustomerSelectedForTicket);
        }

        private void OnFindTicket(string obj)
        {
            AppServices.MainDataContext.OpenTicketFromTicketNumber(TicketSearchText);
            if (AppServices.MainDataContext.SelectedTicket != null)
                AppServices.MainDataContext.SelectedTicket.PublishEvent(EventTopicNames.TicketSelectedFromTableList);
            TicketSearchText = "";
        }

        private bool CanFindTicket(string arg)
        {
            return !string.IsNullOrEmpty(TicketSearchText) && SelectedTicket == null;
        }

        private bool CanCreateCustomer(string arg)
        {
            return SelectedCustomer == null;
        }

        private void OnCreateCustomer(string obj)
        {
            FoundCustomers.Clear();
            var c = new Customer
                        {
                            Address = AddressSearchText,
                            Name = CustomerNameSearchText,
                            PhoneNumber = PhoneNumberSearchText
                        };
            FoundCustomers.Add(new CustomerViewModel(c));
            SelectedView = 1;
            RaisePropertyChanged("SelectedCustomer");
        }

        private bool CanSelectCustomer(string arg)
        {
            return
                AppServices.MainDataContext.IsCurrentWorkPeriodOpen
                && SelectedCustomer != null
                && !string.IsNullOrEmpty(SelectedCustomer.PhoneNumber)
                && !string.IsNullOrEmpty(SelectedCustomer.Address)
                && !string.IsNullOrEmpty(SelectedCustomer.Name)
                && (AppServices.MainDataContext.SelectedTicket == null || AppServices.MainDataContext.SelectedTicket.CustomerId == 0);
        }

        private void SaveSelectedCustomer()
        {
            if (!SelectedCustomer.IsNotNew)
            {
                var ws = WorkspaceFactory.Create();
                ws.Add(SelectedCustomer.Model);
                ws.CommitChanges();
            }
        }

        private void OnSelectCustomer(string obj)
        {
            SaveSelectedCustomer();
            SelectedCustomer.Model.PublishEvent(EventTopicNames.CustomerSelectedForTicket);
            ClearSearchValues();
        }

        void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _updateTimer.Stop();
            UpdateFoundCustomers();
        }

        private void ResetTimer()
        {
            _updateTimer.Stop();

            if (!string.IsNullOrEmpty(PhoneNumberSearchText)
                || !string.IsNullOrEmpty(CustomerNameSearchText)
                || !string.IsNullOrEmpty(AddressSearchText))
            {
                _updateTimer.Start();
            }
            else FoundCustomers.Clear();
        }

        private void UpdateFoundCustomers()
        {

            IEnumerable<Customer> result = new List<Customer>();

            using (var worker = new BackgroundWorker())
            {
                worker.DoWork += delegate
                                     {
                                         bool searchPn = string.IsNullOrEmpty(PhoneNumberSearchText);
                                         bool searchCn = string.IsNullOrEmpty(CustomerNameSearchText);
                                         bool searchAd = string.IsNullOrEmpty(AddressSearchText);

                                         result = Dao.Query<Customer>(
                                             x =>
                                                (searchPn || x.PhoneNumber.Contains(PhoneNumberSearchText)) &&
                                                (searchCn || x.Name.ToLower().Contains(CustomerNameSearchText.ToLower())) &&
                                                (searchAd || x.Address.ToLower().Contains(AddressSearchText.ToLower())));
                                     };

                worker.RunWorkerCompleted +=
                    delegate
                    {

                        AppServices.MainDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                               delegate
                               {
                                   FoundCustomers.Clear();
                                   FoundCustomers.AddRange(result.Select(x => new CustomerViewModel(x)));

                                   if (SelectedCustomer != null && PhoneNumberSearchText == SelectedCustomer.PhoneNumber)
                                   {
                                       SelectedView = 1;
                                       SelectedCustomer.UpdateDetailedInfo();
                                   }

                                   RaisePropertyChanged("SelectedCustomer");

                                   CommandManager.InvalidateRequerySuggested();
                               }));

                    };

                worker.RunWorkerAsync();
            }
        }

        private void OnCloseScreen(string obj)
        {
            if (AppServices.MainDataContext.SelectedDepartment != null && AppServices.MainDataContext.IsCurrentWorkPeriodOpen)
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicketView);
            else
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateNavigation);
            SelectedView = 0;
            ActiveView = 0;
            SelectedCustomerTransactions.Clear();
        }

        public void RefreshSelectedCustomer()
        {
            ClearSearchValues();

            if (AppServices.MainDataContext.SelectedTicket != null && AppServices.MainDataContext.SelectedTicket.CustomerId > 0)
            {
                var customer = Dao.SingleWithCache<Customer>(x => x.Id == AppServices.MainDataContext.SelectedTicket.CustomerId);
                if (customer != null) FoundCustomers.Add(new CustomerViewModel(customer));
                if (SelectedCustomer != null)
                {
                    SelectedView = 1;
                    SelectedCustomer.UpdateDetailedInfo();
                }
            }
            RaisePropertyChanged("SelectedCustomer");
            RaisePropertyChanged("IsClearVisible");
            RaisePropertyChanged("IsResetCustomerVisible");
            RaisePropertyChanged("IsMakePaymentVisible");
            ActiveView = 0;
            SelectedCustomerTransactions.Clear();
        }

        private void ClearSearchValues()
        {
            FoundCustomers.Clear();
            SelectedView = 0;
            ActiveView = 0;
            PhoneNumberSearchText = "";
            AddressSearchText = "";
            CustomerNameSearchText = "";
        }

        public void SearchCustomer(string phoneNumber)
        {
            ClearSearchValues();
            PhoneNumberSearchText = phoneNumber;
            UpdateFoundCustomers();
        }
    }
}

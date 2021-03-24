using CSharp.NFC;
using CSharp.NFC.Cards;
using CSharp.NFC.Controllers;
using CSharp.NFC.Readers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using NFCTicketing;
using Windows.Devices.SmartCards;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NFCTicketValidator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private NFCReader _ticketValidator;
        private TicketingService _ticketService;
        private ValidatorPrototypeViewModel _viewModel;

        public MainPage()
        {
            _viewModel = new ValidatorPrototypeViewModel(this);
            this.InitializeComponent();
            this.InitializeValidator();
        }

        private async void InitializeValidator()
        {
            _ticketValidator = await NFCReader.Helper.GetReader<ACR122, PN532, WindowsLogger>();
            _ticketValidator.BuiltinReader.CardAdded += CardAdded;
            IReadOnlyList<SmartCard> cards = await _ticketValidator.BuiltinReader.FindAllCardsAsync();
            if (cards.Count > 0)
            {
                ConnectCard();
            }
        }

        private void CardAdded(SmartCardReader sender, CardAddedEventArgs args)
        {
            ConnectCard();
        }

        private void ConnectCard()
        {
            _ticketValidator.ConnectCard<Ntag215>();
            _ticketService = new TicketingService(_ticketValidator, _ticketValidator.ConnectedCard.CardUIDBytes, new BusLocation("home"));
            // Uncomment below to reset ticket fast
            //_ticketService.ResetTicket();
            //
            _ticketService.ConnectTicket();
            RefreshTicketValue();
        }

        private void btnValidateTicket_Click(object sender, RoutedEventArgs e)
        {
            _ticketService.ValidateTicket();
            RefreshTicketValue();
        }

        private void btnAddCredit_Click(object sender, RoutedEventArgs e)
        {
            _ticketService.AddCredit(decimal.Parse(txtboxCredit.Text));
            RefreshTicketValue();
        }

        private void btnResetTicket_Click(object sender, RoutedEventArgs e)
        {
            _ticketService.InitNewTicket();
            RefreshTicketValue();
        }

        private void bntReadTicket_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Ticket = _ticketService.ReadTicket();
        }

        private void RefreshTicketValue() 
        {
            _viewModel.Ticket = _ticketService.ConnectedTicket;
        }
    }

    public class BindableBase : INotifyPropertyChanged
    {
        protected MainPage _page;
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            _page?.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => this.PropertyChanged?.DynamicInvoke(this, new PropertyChangedEventArgs(propertyName)));
        }
    }

    public class ValidatorPrototypeViewModel : BindableBase
    {
        private EncryptableSmartTicket _ticket;

        public EncryptableSmartTicket Ticket
        {
            get { return _ticket; }
            set { this._ticket = value; OnPropertyChanged(); }
        }

        public ValidatorPrototypeViewModel(MainPage page)
        {
            _ticket = new EncryptableSmartTicket();
            _page = page;
        }
    }
}

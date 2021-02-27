using CSharp.NFC;
using CSharp.NFC.Cards;
using CSharp.NFC.Controllers;
using CSharp.NFC.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Ticketing;
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
        private NFCReader TicketValidator;
        public SmartTicket Ticket;

        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeValidator();
        }

        private async void InitializeValidator()
        {
            TicketValidator = await NFCReader.Helper.GetReader<ACR122, PN532, WindowsLogger>();
            TicketValidator.BuiltinReader.CardAdded += CardAdded;
            IReadOnlyList<SmartCard> cards = await TicketValidator.BuiltinReader.FindAllCardsAsync();
            if (cards.Count > 0)
            {
                TicketValidator.ConnectCard<Ntag215>();
                //WriteCardInfo(cards.First());
            }
        }


        private void CardAdded(SmartCardReader sender, CardAddedEventArgs args)
        {
            SmartCard card = args.SmartCard;
            TicketValidator.ConnectCard<Ntag215>();
            TicketingService service = new TicketingService(TicketValidator, TicketValidator.ConnectedCard.CardUIDBytes);
            //WriteCardInfo(card);
        }
    }
}

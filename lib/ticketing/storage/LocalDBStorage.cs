using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCTicketing
{
    public class LocalDBStorage : IValidationStorage
    {
        private string _connectionString = @"Data Source=USER-PC\SQLEXPRESS;Initial Catalog=NFCValidationStorage;Integrated Security=SSPI";

        public void RegisterValidation(TicketValidation validation)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                string cardIDParameter = "@cardid";
                string locationParameter = "@location";
                string validationTimeParameter = "@validationTime";
                string encryptedTicketHashParameter = "@encryptedTicketHash";
                string commandString = $"INSERT INTO Validation (card_id, location, validation_time, encrypted_ticket) VALUES ({cardIDParameter}, {locationParameter}, {validationTimeParameter}, {encryptedTicketHashParameter})";
                SqlCommand command = new SqlCommand(commandString, connection);
                command.Parameters.AddWithValue(cardIDParameter, BitConverter.ToString(validation.CardID));
                command.Parameters.AddWithValue(locationParameter, validation.Location);
                command.Parameters.AddWithValue(validationTimeParameter, validation.Time);
                command.Parameters.AddWithValue(encryptedTicketHashParameter, validation.EncryptedTicketHash);
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch(Exception ex) { }            
        }

        public void RegisterTicketUpdate(SmartTicket ticket)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();                
                string creditParameter = "@credit";
                string ticketTypeParameter = "@ticketType";
                string currentValidationParameter = "@currentValidation";
                string sessionValidationParameter = "@sessionValidation";
                string sessionExpenseParameter = "@sessionExpense";
                string cardIDParameter = "@cardid";
                string commandString = $"UPDATE SmartTicket SET credit = {creditParameter}, ticket_type = {ticketTypeParameter}, current_validation = {currentValidationParameter}, session_validation = {sessionValidationParameter}, session_expense = {sessionExpenseParameter} WHERE card_id = {cardIDParameter}";
                SqlCommand command = new SqlCommand(commandString, connection);                
                command.Parameters.AddWithValue(creditParameter, ticket.Credit);
                command.Parameters.AddWithValue(ticketTypeParameter, ticket.TicketTypeName);
                command.Parameters.AddWithValue(currentValidationParameter, ticket.CurrentValidation);
                command.Parameters.AddWithValue(sessionValidationParameter, ticket.SessionValidation);
                command.Parameters.AddWithValue(sessionExpenseParameter, ticket.SessionExpense);
                command.Parameters.AddWithValue(cardIDParameter, BitConverter.ToString(ticket.CardID));
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex) { }
        }
    }
}

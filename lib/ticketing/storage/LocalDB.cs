using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCTicketing
{
    public class LocalDB : IValidationStorage
    {
        private string _connectionString = @"Data Source=USER-PC\SQLEXPRESS;Initial Catalog=NFCValidationStorage;Integrated Security=True";
        public void RegisterLocation(string location, byte[] cardID)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                string cardIDParameterName = "@cardid";
                string locationParameterName = "@location";
                string commandString = $"INSERT INTO Validation (CardID, Location) VALUES ({cardIDParameterName}, {locationParameterName})";
                SqlCommand command = new SqlCommand(commandString, connection);
                command.Parameters.AddWithValue(cardIDParameterName, BitConverter.ToString(cardID));
                command.Parameters.AddWithValue(locationParameterName, BitConverter.ToString(cardID));
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch(Exception ex)
            {

            }            
        }
    }
}

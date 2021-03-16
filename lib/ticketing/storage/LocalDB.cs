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
        private string _connectionString = @"Data Source=USER-PC\SQLEXPRESS;Initial Catalog=NFCValidationStorage;Integrated Security=SSPI";
        public void RegisterLocation(string location, byte[] cardID, DateTime validationTime)
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                string cardIDParameterName = "@cardid";
                string locationParameterName = "@location";
                string validationTimeParameterName = "@validationTime";
                string commandString = $"INSERT INTO Validation (CardID, Location, ValidationTime) VALUES ({cardIDParameterName}, {locationParameterName}, {validationTimeParameterName})";
                SqlCommand command = new SqlCommand(commandString, connection);
                command.Parameters.AddWithValue(cardIDParameterName, BitConverter.ToString(cardID));
                command.Parameters.AddWithValue(locationParameterName, location);
                command.Parameters.AddWithValue(validationTimeParameterName, validationTime);
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch(Exception) { }            
        }
    }
}

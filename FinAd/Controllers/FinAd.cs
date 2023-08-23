using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
//using System.Web.Script.Serialization;

namespace FinAd.Controllers
{
    [ApiController]
    [Route("/api")]
    
    public class FinAd : ControllerBase
    {
        private IConfiguration _config;
        public FinAd(IConfiguration config)
        {
            _config = config;
        }

        // Adding advisors info into Advisor table
        [HttpPost("/advisorSignup")]
        public IActionResult PostClient(User user)
        {
            /*var advisorList = new List<advisorSignup>();*/
            Console.WriteLine(user.Email + " " + user.Password);

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";
            using (SqlConnection connection = new(connnectionString))
            {
                    try
                    {
                        String sql = "INSERT INTO advisorSignup (Name, PhoneNo, Email, Address, Username, Password) " +
                        "VALUES('"+ user.Name + "', '"+ user.PhoneNo + "', '"+ user.Email + "', '"+ user.Address + "', '"+ user.Username + "', '"+user.Password+"')";
                        connection.Open();
                        using (SqlCommand command = new(sql, connection))
                        {
                        {
                            Console.WriteLine(sql);
                            command.ExecuteNonQuery();
                            Console.WriteLine("string");
                        }
                    }
                            

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                    }

                

                
            }

            
            // return _employeeRepository.GetAllEmployee();
            //  Console.WriteLine(employeeLis.Name +  "   " + employeeLis.Count);
            /*Console.WriteLine(JsonConvert.SerializeObject(clientsList));
            var val = JsonConvert.SerializeObject(clientsList);
            Console.WriteLine(val);
            return val;*/
            return Ok("done");
        }

        // returning advisor data if the advisor credentials are true
      //  [HttpPost]
        

        



        //Checking if the email and password matches in the Advisor table
        [HttpPost("/advisorLogin")]

        public IActionResult AdvisorData(Login login)
        {
            var advisor = Authenticate(login);
            if (advisor != null)
            {
                var token = Generate(advisor);
                var tokken = token?.GetType().GetProperty("Value")?.GetValue(token, null);
                Console.WriteLine(JsonConvert.SerializeObject(token));
                return Ok(JsonConvert.SerializeObject(token));
            }
            else
            {
                return NotFound("User not found");
            }

        }

        public Object Generate(AdvisorData advisor)
        {
            Console.WriteLine(advisor.Email);
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Jwt:Key"]));
            var Credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                /*new Claim(ClaimTypes.NameIdentifier, advisor.Name),*/
                /*new Claim(ClaimTypes.MobilePhone, advisor.PhoneNo),*/
                new Claim(ClaimTypes.Email, advisor.Email)
                /*new Claim(ClaimTypes.StreetAddress, advisor.Address),
                new Claim(ClaimTypes.Surname, advisor.Username),
                new Claim(ClaimTypes.Thumbprint, advisor.Password)*/

            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"], 
                _config["Jwt:Audience"],
                claims,
                DateTime.UtcNow,
                DateTime.Now.AddMinutes(20),
                Credentials);
            var obj = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(obj);
        }

        public AdvisorData Authenticate(Login login)
        {
            AdvisorData user =null;
            /*var advisorList = new List<advisorSignup>();*/
            Console.WriteLine(login.Email + " " + login.Password);


            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";
            using (SqlConnection connection = new(connnectionString))
            
            {
                try
                {
                   string sql = "SELECT Email, Password FROM advisorSignup WHERE Email = '" + login.Email + "' AND Password = '" + login.Password + "'";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        AdvisorData usr = new AdvisorData();
                        usr.Email = rdr.GetString(0);
                        user = usr;
                        
                    }

                    /*SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM advisorSignup WHERE Email = '" + login.Email + "'  AND Password = '" + login.Password + "' ", connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    if (dataTable.Rows.Count > 0)
                    {
                        return dataTable;
                    }
                    else
                    {
                        return NotFound("Invalid User");
                    }*/
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    connection.Close();
                    return user;

                }
            }
            return user;
        }

        // Adding new client data into clients Table
        [HttpPost("/newClientData")]
        public IActionResult PostClient(newClientsData newClient)
        {
            /*var advisorList = new List<advisorSignup>();*/
            Console.WriteLine(newClient.Email + " " + newClient.RiskProfile);

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";
            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "INSERT INTO clientsData (Name, PhoneNo, Email, PancardNo, AadharNo, RiskProfile, SuggestedModel) " +
                    "VALUES('" + newClient.Name + "', '" + newClient.PhoneNo + "', '" + newClient.Email + "', '" + newClient.PancardNo + "', '" + newClient.AadharNo + "', '" + newClient.RiskProfile + "', '" + newClient.SuggestedModel + "')";
                    connection.Open();
                    using (SqlCommand command = new(sql, connection))
                    {
                        {
                            Console.WriteLine(sql);
                            command.ExecuteNonQuery();
                        }
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }
            }


           
            return Ok("Client Added Successfully");
        }






        // Fetching all clients details
        [Authorize]
        [HttpGet("/existingClientsData")]
        public OkObjectResult GetAllClients()
        {
            List<ExistingClientsData> clientList = new List<ExistingClientsData>();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";

            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "SELECT * FROM  clientsData ";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        ExistingClientsData client = new ExistingClientsData();
                        client.Name = rdr["Name"].ToString();
                        client.Email = rdr["Email"].ToString();
                        client.PhoneNo = Convert.ToInt32(rdr["PhoneNo"]);
                        client.PancardNo = rdr["PancardNo"].ToString();
                        client.AadharNo = Convert.ToInt32(rdr["AadharNo"]);
                        client.RiskProfile = rdr["RiskProfile"].ToString();
                        client.SuggestedModel = rdr["SuggestedModel"].ToString();
                        clientList.Add(client);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }

            }

            // JavaScriptSerializer js = new JavaScriptSerializer();
            //  Context.Response.Write(js.Serialize(ExistingClientsData));
            var clientInfo = JsonConvert.SerializeObject(clientList);
            return Ok(clientInfo);
        }




        [HttpGet("/securitiesList")]
        public OkObjectResult GetAllSecurities()
        {
            List<SecuritiesList> securityList = new List<SecuritiesList>();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";

            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "SELECT * FROM  listOfSecurities ";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        SecuritiesList security = new SecuritiesList();
                        security.Securities = rdr["Security_Name"].ToString();
                       
                        securityList.Add(security);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }

            }

            // JavaScriptSerializer js = new JavaScriptSerializer();
            //  Context.Response.Write(js.Serialize(ExistingClientsData));
            var securityInfo = JsonConvert.SerializeObject(securityList);
            return Ok(securityInfo);
        }


        [HttpGet("/selectedSecuritiesList")]
        public OkObjectResult GetSelectecSecurities()
        {
            List<SecuritiesList> securityList = new List<SecuritiesList>();

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";

            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "SELECT * FROM  listOfSecurities ";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        SecuritiesList security = new SecuritiesList();
                        security.Securities = rdr["Security_Name"].ToString();

                        securityList.Add(security);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }

            }

            // JavaScriptSerializer js = new JavaScriptSerializer();
            //  Context.Response.Write(js.Serialize(ExistingClientsData));
            var securityInfo = JsonConvert.SerializeObject(securityList);
            return Ok(securityInfo);
        }



        // adding model securities and weightage to datatbase
        [HttpPost("/newModelData")]
        public IActionResult ModelData(NewModelData newModel)
        {
            /*var advisorList = new List<advisorSignup>();*/
            /*Console.WriteLine(newClient.Email + " " + newClient.RiskProfile);*/

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";
            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "INSERT INTO Models (AdvisorEmail, RiskProfile, ModelName, Securities, Weightage) " +
                    "VALUES('" + newModel.AdvisorEmail + "', '" + newModel.RiskProfile + "', '" + newModel.ModelName + "', '" + newModel.Securities + "', '" + newModel.Weightage + "')";
                    connection.Open();
                    using (SqlCommand command = new(sql, connection))
                    {
                        {
                            Console.WriteLine(sql);
                            command.ExecuteNonQuery();
                        }
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }
            }



            return Ok("Model Added Successfully");
        }
    }
}

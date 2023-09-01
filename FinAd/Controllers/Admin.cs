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

namespace FinAd.Controllers
{
    
    [ApiController]
    [Route("/api")]

    public class Admin : ControllerBase
    {

        private IConfiguration _config;
        public Admin(IConfiguration config)
        {
            _config = config;
        }


        private AdminModel GetCurrentAdmin()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var adminClaims = identity.Claims;

                return new AdminModel
                {
                    AdminEmail = adminClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,

                };
            }
            return null;
        }



        //Checking if the email and password matches in the Advisor table and login
        [HttpPost("/adminLogin")]

        public IActionResult AdvisorData(AdminLogin adminLogin)
        {
            var admin = Authenticate(adminLogin);
            if (admin != null)
            {
                var token = Generate(admin);
                var tokken = token?.GetType().GetProperty("Value")?.GetValue(token, null);
                HttpContext.Response.Headers.Add("token", tokken.ToString());
                /*Console.WriteLine(JsonConvert.SerializeObject(token));*/
                return Ok(true);
            }
            else
            {
                return NotFound("Admin not found");
            }

        }

        public Object Generate(AdminData admin)
        {
            Console.WriteLine(admin.Email);
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Jwt:Key"]));
            var Credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, admin.Email)
                

            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                DateTime.UtcNow,
                DateTime.Now.AddMinutes(30),
                Credentials);
            var obj = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(obj);
        }

        public AdminData Authenticate(AdminLogin adminLogin)
        {
            AdminData user = null;
            /*var advisorList = new List<advisorSignup>();*/
            Console.WriteLine(adminLogin.Email + " " + adminLogin.Password);


            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";
            using (SqlConnection connection = new(connnectionString))

            {
                try
                {
                    string sql = "SELECT Email, Password FROM adminLogin WHERE Email = '" + adminLogin.Email + "' AND Password = '" + adminLogin.Password + "'";
                    SqlCommand command = new SqlCommand(sql, connection);
                    connection.Open();
                    SqlDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        AdminData usr = new AdminData();
                        usr.Email = rdr.GetString(0);
                        user = usr;

                    }
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


        // Delete existing Securites in database
        /*[Authorize]*/
        [HttpDelete("/deleteSecurities")]
        public IActionResult DeleteScurities()
        {
           /* var admin = GetCurrentAdmin();*/

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";
            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "DELETE FROM listofSecurities";
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
                    return NotFound("Not Deleted");
                }
            }
            return Ok("Securities Deleted Successfully");
        }




        // Delete existing Securites in database
        /*[Authorize]*/
        [HttpPost("/addAllSecurities")]
        public IActionResult AddScurities(SecuritiesList securities)
        {
            /*var admin = GetCurrentAdmin();*/

            String connnectionString = "Data Source=L-3Q8PHR3\\SQLEXPRESS;Initial Catalog=FinAdDatabase;Integrated Security=True";
            using (SqlConnection connection = new(connnectionString))
            {
                try
                {
                    String sql = "INSERT INTO listOfSecurities (Asset_Class, Security_Name) " + "VALUES('" + securities.AssetClass + "', '" + securities.Securities + "')";
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
                    return NotFound("Not Added");
                }
            }
            return Ok("Securities Added Successfully");
        }


    }
}

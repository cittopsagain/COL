using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Text;

namespace COL
{
    class Program
    {
        static void Main(string[] args)
        {
            MySqlConnection conn = MySQLDatabase.getConnection();
            if (conn == null)
            {
                Console.WriteLine("Could not connect to database!");
                Environment.Exit(0);
            }

            Menu.display(conn);
        }
    }

    class Menu
    {
        private static readonly string SUCCESS_SAVE = "Record successfully saved!";
        private static readonly string ERROR_SAVE = "Problem while saving the record!";
        private static readonly string REC_EXIST = "code already exist!";
        private static readonly string REC_NOT_EXIST = "code doest not exist!";
        private static readonly string INVALID_OPTION = "Invalid option!";
        static Menu()
        {

        }

        private static string strRepeat(string value, int max)
        {
            // https://stackoverflow.com/questions/411752/best-way-to-repeat-a-character-in-c-sharp
            return new StringBuilder(max).Insert(0, " ", max - value.Length).ToString();
        }

        public static void display(MySqlConnection conn)
        {
            int option = 0;
            Console.WriteLine("[1] New Company\r\n[2] Stocks \r\n[3] Portfolio\r\n[0] Exit");
            do
            {
                try
                {
                    option = Convert.ToInt32(Console.ReadLine());
                } catch(Exception ex)
                {
                    Console.WriteLine(INVALID_OPTION);
                    display(conn);
                }
                
                if (option == 1)
                {
                    Console.WriteLine("* New Company *");
                    string compCode;
                    string compName;
                    Console.WriteLine("Company Code: ");
                    compCode = Console.ReadLine();
                    Console.WriteLine("Company Name: ");
                    compName = Console.ReadLine();
                    if (Companies.isCompCodeExist(conn, compCode))
                    {
                        Console.WriteLine(compCode+" "+ REC_EXIST);
                        Menu.display(conn);
                        return;
                    }
                    if (Companies.insert(conn, compCode, compName))
                    {
                        Console.WriteLine(SUCCESS_SAVE);
                    }
                    else
                    {
                        Console.WriteLine(ERROR_SAVE);
                    }
                    Menu.display(conn);
                }
                else if (option == 2)
                {
                    Console.WriteLine("* Stocks *");
                    string compCode;
                    double price;
                    int qty;
                    Console.WriteLine("Company Code: ");
                    compCode = Console.ReadLine();
                    Console.WriteLine("Price: ");
                    try
                    {
                        price = Convert.ToDouble(Console.ReadLine());
                    } catch(Exception ex)
                    {
                        price = 0.00;
                    }
                    
                    Console.WriteLine("Qty:");
                    try
                    {
                        qty = Convert.ToInt32(Console.ReadLine());
                    } catch(Exception ex)
                    {
                        qty = 0;
                    }
                    
                    if (!Companies.isCompCodeExist(conn, compCode))
                    {
                        Console.WriteLine(compCode+" "+ REC_NOT_EXIST);
                        Menu.display(conn);
                        return;
                    }
                    if (Stocks.insert(conn, compCode, price, qty))
                    {
                        Console.WriteLine(SUCCESS_SAVE);
                    } else
                    {
                        Console.WriteLine(ERROR_SAVE);
                    }
                    Menu.display(conn);
                } else if (option == 3)
                {
                    string compCode = "";
                    Console.WriteLine("* Portfolio *");
                    Console.WriteLine("Company Code: ");
                    compCode = Console.ReadLine();
                    if (!Companies.isCompCodeExist(conn, compCode))
                    {
                        Console.WriteLine(compCode+" "+ REC_NOT_EXIST);
                        Menu.display(conn);
                    } else
                    {
                        string compName = "";
                        MySqlDataReader compRdr = Companies.getCompanyByCode(conn, compCode);
                        while (compRdr.Read())
                        {
                            compName = compRdr["comp_name"].ToString();
                        }
                        compRdr.Close();
                        Console.WriteLine(compName);
                        MySqlDataReader rdr = Stocks.getStocks(conn);
                        // TO DO: Must dynamic
                        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------\r\n" +
                            "+     Date     |   Price   |   Qty   |  Amount Invested  |  Total Amount Invested  |  Total Qty  |  Stock Value  |  Gain or Loss  +\r\n"+
                            "-----------------------------------------------------------------------------------------------------------------------------------");
                        DataTable dt = new DataTable();
                        dt.Load(compRdr);
                        for (int i = 0; i <= dt.Rows.Count; i++)
                        {
                            Console.WriteLine("Here: "+dt.Rows[i]);
                        }
                        /* while (rdr.Read())
                        {
                            DateTime dt = DateTime.Parse(rdr["date"].ToString());
                            int qty = Convert.ToInt32(rdr["qty"]);
                            double price = Convert.ToDouble(rdr["price"]);
                            double amtInvested = qty * price;
                            Console.WriteLine("| "+dt.ToString("MM/dd/yyyy") +"   | "+
                                price+Menu.strRepeat(price.ToString(), 10)+"| "+
                                qty+Menu.strRepeat(qty.ToString(), 8)+"| "+
                                amtInvested+Menu.strRepeat(amtInvested.ToString(), 18)+"| ");
                        } */
                        rdr.Close();
                    }
                }
                else if (option == 0)
                {
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine(INVALID_OPTION);
                }
            } while (option != 1 || option != 2 || option != 3 || option != 0);
        }
    }

    class MySQLDatabase
    {
        private static MySqlConnection conn = null;
        static MySQLDatabase()
        {
            string connStr = "server=localhost;user=root;database=col;port=3306;password=clearex";
            conn = new MySqlConnection(connStr);
        }

        public static MySqlConnection getConnection()
        {
            try
            {
                conn.Open();
                return conn;
            } catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }
    }

    class Companies
    {
        static Companies()
        {
           
        }

        public static bool insert(MySqlConnection conn, string compCode, string compName)
        {
            string sql = "INSERT INTO companies (comp_code, comp_name) VALUES (@CompCode, @CompName)";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CompCode", compCode);
            cmd.Parameters.AddWithValue("@CompName", compName);
            if (cmd.ExecuteNonQuery() < 1)
            {
                return false;
            }

            return true;
        }

        public static MySqlDataReader getCompanyByCode(MySqlConnection conn, string compCode)
        {
            string sql = "SELECT * FROM companies WHERE comp_code = @CompCode";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CompCode", compCode);
            MySqlDataReader rdr = cmd.ExecuteReader();

            return rdr;
        }

        public static bool isCompCodeExist(MySqlConnection conn, string compCode)
        {
            string sql = "SELECT * FROM companies WHERE comp_code = @CompCode";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CompCode", compCode);
            MySqlDataReader rdr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(rdr);
            if (dt.Rows.Count > 0)
            {
                return true;
            }

            rdr.Close();
            return false;
        }
    }

    class Stocks
    {
        static Stocks()
        {

        }

        public static bool insert(MySqlConnection conn, String compCode, double price, int qty)
        {
            string sql = "INSERT INTO stocks (comp_code, price, qty, date) VALUES (@CompCode, @Price, @Qty, @Date)";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CompCode", compCode);
            cmd.Parameters.AddWithValue("@Price", price);
            cmd.Parameters.AddWithValue("@Qty", qty);
            cmd.Parameters.AddWithValue("@Date", DateTime.Now);
            if (cmd.ExecuteNonQuery() < 1)
            {
                return false;
            }

            return true;
        }

        public static MySqlDataReader getStocks(MySqlConnection conn)
        {
            string sql = "SELECT * FROM stocks";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            return rdr;
        }
    }
}

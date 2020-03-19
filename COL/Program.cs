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
        private static readonly string DATE_FORMAT = "MM/dd/yyyy";
        static Menu()
        {

        }

        private static string strRepeat(string value, int max)
        {
            // https://stackoverflow.com/questions/411752/best-way-to-repeat-a-character-in-c-sharp
            return new StringBuilder(max).Insert(0, " ", max - value.Length).ToString();
        }

        private static string parseDate(string date, string format)
        {
            return DateTime.Parse(date).ToString(format);
        }

        public static void display(MySqlConnection conn)
        {
            int option = 0;
            Console.WriteLine("[1] New Company\r\n" +
                "[2] View Companies\r\n" +
                "[3] Buy Stocks\r\n" +
                "[4] Portfolio\r\n" +
                "[5] Deposit\r\n" +
                "[6] View Deposits\r\n" +
                "[7] Cash Balance\r\n" +
                "[0] Exit");
            do
            {
                try
                {
                    option = Convert.ToInt32(Console.ReadLine());
                }
                catch (Exception ex)
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
                        Console.WriteLine(compCode + " " + REC_EXIST);
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
                    Console.WriteLine("* View Companies *");
                    MySqlDataReader rdr = Companies.getAllCompanies(conn);
                    // TO DO: Must be dynamic
                    Console.WriteLine("+-----------------+--------------------------------------------+\r\n" +
                        "+ Company Code    | Company Description                        +\r\n"+
                        "+-----------------+--------------------------------------------+");
                    while (rdr.Read() && rdr != null)
                    {
                        string compCode = rdr["comp_code"].ToString();
                        string compName = rdr["comp_name"].ToString();
                        Console.WriteLine("| "+ compCode + strRepeat(compCode, 15)+" | "+compName+ strRepeat(compName, 43) +"|");
                    }
                    Console.WriteLine("+-----------------+--------------------------------------------+");

                    rdr.Close();
                }
                else if (option == 3)
                {
                    Console.WriteLine("* Purchase Stocks *");
                    string compCode;
                    double price;
                    int qty;
                    Console.WriteLine("Company Code: ");
                    compCode = Console.ReadLine();
                    Console.WriteLine("Price: ");
                    try
                    {
                        price = Convert.ToDouble(Console.ReadLine());
                    }
                    catch (Exception ex)
                    {
                        price = 0.00;
                    }

                    Console.WriteLine("Qty:");
                    try
                    {
                        qty = Convert.ToInt32(Console.ReadLine());
                    }
                    catch (Exception ex)
                    {
                        qty = 0;
                    }

                    if (!Companies.isCompCodeExist(conn, compCode))
                    {
                        Console.WriteLine(compCode + " " + REC_NOT_EXIST);
                        Menu.display(conn);
                        return;
                    }
                    if (Stocks.insert(conn, compCode, price, qty))
                    {
                        Console.WriteLine(SUCCESS_SAVE);
                    }
                    else
                    {
                        Console.WriteLine(ERROR_SAVE);
                    }
                    Menu.display(conn);
                }
                else if (option == 4)
                {
                    string compCode = "";
                    Console.WriteLine("* Portfolio *");
                    Console.WriteLine("Company Code: ");
                    compCode = Console.ReadLine();
                    if (!Companies.isCompCodeExist(conn, compCode))
                    {
                        Console.WriteLine(compCode + " " + REC_NOT_EXIST);
                        Menu.display(conn);
                    }
                    else
                    {
                        string compName = "";
                        MySqlDataReader compRdr = Companies.getCompanyByCompCode(conn, compCode);
                        while (compRdr.Read())
                        {
                            compName = compRdr["comp_name"].ToString();
                        }
                        compRdr.Close();
                        Console.WriteLine(compName);
                        Console.WriteLine("Note: Comm + VAT and Other Charges are not included in the calculation!");
                        MySqlDataReader rdr = Stocks.getStocksByCompCode(conn, compCode);
                        // TO DO: Must be dynamic
                        Console.WriteLine("+--------------+-----------+---------+-------------------+-------------------------+-------------+---------------+----------------+\r\n" +
                            "+     Date     |   Price   |   Qty   |  Amount Invested  |  Total Amount Invested  |  Total Qty  |  Stock Value  |  Gain or Loss  +\r\n" +
                            "+--------------+-----------+---------+-------------------+-------------------------+-------------+---------------+----------------+");
                        DataTable dtStocks = new DataTable(); // Optional to use the DataTable here, we can use directly the rdr.Read()
                        dtStocks.Load(rdr);

                        double totalAmtInvested = 0.00;
                        int totalQty = 0;
                        String data = "";
                        for (int i = 0; i < dtStocks.Rows.Count; i++)
                        {
                            int qty = Convert.ToInt32(dtStocks.Rows[i]["qty"]);
                            double price = Convert.ToDouble(dtStocks.Rows[i]["price"]);
                            double amtInvested = qty * price;
                            totalAmtInvested += amtInvested;
                            totalQty += qty;

                            double stockValue = totalQty * price;
                            double gainOrLoss = stockValue - totalAmtInvested;

                            data += "| " + parseDate(dtStocks.Rows[i]["date"].ToString(), DATE_FORMAT) + "   | " +
                                price + strRepeat(price.ToString(), 10) + "| " +
                                qty + strRepeat(qty.ToString(), 8) + "| " +
                                Math.Round(amtInvested, 2) + strRepeat(amtInvested.ToString(), 18) + "| " +
                                Math.Round(totalAmtInvested, 2) + strRepeat(totalAmtInvested.ToString(), 24) + "| " +
                                totalQty + strRepeat(totalQty.ToString(), 12) + "| " +
                                Math.Round(stockValue, 2) + strRepeat(stockValue.ToString(), 14) + "| " +
                                Math.Round(gainOrLoss, 2) + strRepeat(gainOrLoss.ToString(), 15) + "| \r\n";
                        }
                        data += "+--------------+-----------+---------+-------------------+-------------------------+-------------+---------------+----------------+";
                        Console.Write(data);

                        rdr.Close();
                    }
                }
                else if (option == 5)
                {
                    Console.WriteLine("* Deposit *");
                    double amt;
                    Console.WriteLine("Amount: ");
                    try
                    {
                        amt = Convert.ToDouble(Console.ReadLine());
                    } catch (Exception ex)
                    {
                        amt = 0.00;
                    }
                    if (Deposits.insert(conn, amt))
                    {
                        Console.WriteLine(SUCCESS_SAVE);
                    }
                    else
                    {
                        Console.WriteLine(ERROR_SAVE);
                    }
                    Menu.display(conn);
                }
                else if (option == 6)
                {
                    Console.WriteLine("* View Deposits *");
                    MySqlDataReader rdr = Deposits.getAllDeposits(conn);
                    // TO DO: Must be dynamic
                    Console.WriteLine("+--------------+-----------+\r\n" +
                        "+     Date     | Amount    +\r\n"+
                        "+--------------+-----------+");
                    double totalDeposit = 0.00;
                    while (rdr.Read() && rdr != null)
                    {
                        double amount = Convert.ToDouble(rdr["amount"]);
                        totalDeposit += amount;
                        Console.WriteLine("| "+ parseDate(rdr["date"].ToString(), DATE_FORMAT)+"   | "+ amount + strRepeat(amount.ToString(), 10)+"|");
                    }
                    Console.WriteLine("+--------------+-----------+");
                    Console.WriteLine("Total Deposit: "+totalDeposit);

                    rdr.Close();
                }
                else if (option == 7)
                {
                    Console.WriteLine("* Cash Balance *");
                    Console.WriteLine("Note: Cash Balance is not equal to COL Account because COMM + VAT & Other Charges is not deducted when buying stocks!");
                    MySqlDataReader rdr = Deposits.getCashBalance(conn);
                    while (rdr.Read() && rdr != null)
                    {
                        Console.WriteLine("Total Deposit: "+rdr["total_deposits"]);
                        Console.WriteLine("Total Invested: "+rdr["total_invested"]);
                        Console.WriteLine("Cash Balance: "+rdr["cash_balance"]);
                    }
                    rdr.Close();
                }
                else if (option == 0)
                {
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine(INVALID_OPTION);
                }
            } while (option != 1 || option != 2 || option != 3 || option != 4 || option != 5 || option != 6 || option != 7 || option != 0);
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
            }
            catch (Exception ex)
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

        public static MySqlDataReader getCompanyByCompCode(MySqlConnection conn, string compCode)
        {
            string sql = "SELECT * FROM companies WHERE comp_code = @CompCode";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CompCode", compCode);
            MySqlDataReader rdr = cmd.ExecuteReader();

            return rdr;
        }

        public static MySqlDataReader getAllCompanies(MySqlConnection conn)
        {
            string sql = "SELECT * FROM companies";
            MySqlCommand cmd = new MySqlCommand(sql, conn);

            return cmd.ExecuteReader();
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

        public static bool insert(MySqlConnection conn, string compCode, double price, int qty)
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

        public static MySqlDataReader getStocksByCompCode(MySqlConnection conn, string compCode)
        {
            string sql = "SELECT * FROM stocks WHERE comp_code = @CompCode";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CompCode", compCode);
            MySqlDataReader rdr = cmd.ExecuteReader();
            
            return rdr;
        }

        public static MySqlDataReader getAllStocks(MySqlConnection conn)
        {
            String sql = "SELECT * FROM stocks";
            MySqlCommand cmd = new MySqlCommand(sql, conn);

            return cmd.ExecuteReader();
        }
    }

    class Deposits
    {
        static Deposits()
        {

        }

        public static bool insert(MySqlConnection conn, double amt)
        {
            String sql = "INSERT INTO deposits (amount, date) VALUES (@Amount, @Date)";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Amount", amt);
            cmd.Parameters.AddWithValue("@Date", DateTime.Now);

            if (cmd.ExecuteNonQuery() < 1)
            {
                return false;
            }

            return true;
        }

        public static MySqlDataReader getCashBalance(MySqlConnection conn)
        {
            String sql = "SELECT *, (total_deposits - total_invested) AS cash_balance FROM (SELECT SUM(total) AS total_invested, " +
                "(SELECT SUM(amount) FROM deposits) AS total_deposits" +
                " FROM(SELECT *, (price * qty) AS total FROM stocks) AS m) main";
            MySqlCommand cmd = new MySqlCommand(sql, conn);

            return cmd.ExecuteReader();
        }

        public static MySqlDataReader getAllDeposits(MySqlConnection conn)
        {
            String sql = "SELECT * FROM deposits";
            MySqlCommand cmd = new MySqlCommand(sql, conn);

            return cmd.ExecuteReader();
        }
    }
}

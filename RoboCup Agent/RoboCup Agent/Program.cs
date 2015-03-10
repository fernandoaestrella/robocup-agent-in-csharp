using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RobocupAttempFernando
{
    class Program
    {

        static void Main(string[] args)
        {
            int time_stay;
            string command, st2;
            double x_target, y_target;
            double x_actual, y_actual;
            double x_var, y_var;
            double deg_target;
            double deg_actual = 0;
            double turn_l = 0;
            double turn_r = 0;
            double turn = 0;
            int vari = 0, CountAr, i = 0, flagInd = 0, check = 0, playerInd = 0;
            double Yplayer = 0;
            double Xplayer = -10;
            double time = 0, bDist = 0, bDirect = 0, bDistChang = 0, bDirChang = 0;
            double[] playerDist = new double[33], playerDirec = new double[33], PlayerDistChang = new double[33],
                PlayerDirectChang = new double[33], playerNumb = new double[33],
                playerBodyFacingDir = new double[33], playerHeadFacingDir = new double[33], FlagDist = new double[33], FlagDirect = new double[33],
                xflag = new double[33], yflag = new double[33];
            string[] delimiterChars = { " " }, delimiterNumb = { ")" }, delimiterplayer = { "\"" }, playerTeamName = new string[33];


            var client = new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6010); // endpoint where server is listening. parameters are (ipaddress, port). make sure server is on the same port

            byte[] b2 = System.Text.Encoding.UTF8.GetBytes("(init teamname (version 15))"); //sends the initialization string. you can change teamname
            var sentData = client.Send(b2, b2.Length, ep);  //sends it to the predetermined endpoint (something that has the form "ip:port" like "127.0.0.1:6010")

            byte[] receiveBytes = client.Receive(ref ep);   //receives from endpoint. ref is used to denote that the server has changed the endpoint (the port of the endpoint) and you use that new endpoint. handshake is finished. client is recognized by the server.
            string returnData = Encoding.ASCII.GetString(receiveBytes); //you receive the information in byte array. turn it to string

            command = "(move -10 0)";   //send this asap to try to sinc asap
            b2 = System.Text.Encoding.UTF8.GetBytes(command);   //Send() only sends byte array
            sentData = client.Send(b2, b2.Length, ep);

            Console.WriteLine("This is the message you received " +
                                        returnData);
            Console.WriteLine("This message was sent from " +
                                        ep.Address.ToString() +
                                        " on their port number " +
                                        ep.Port.ToString());

            Thread.Sleep(20); //delay between Send's. can't send too many too soon


            i = 0;
            do
            {
                try
                {
                    receiveBytes = client.Receive(ref ep);
                    returnData = Encoding.ASCII.GetString(receiveBytes);
                    Thread.Sleep(200);
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e.Message);
                }
                i++;
            }
            while (i < 22);


            //the Big While. never gets out of this. asks for a position (in a range "normalized": from 0 to 104 in X, and from 0 to 34 in Y)
            while (true)
            {
                Console.WriteLine("Insert desired X coordinate");
                st2 = Console.ReadLine();
                x_target = Convert.ToInt32(st2);

                Console.WriteLine("Insert desired Y coordinate");
                st2 = Console.ReadLine();
                y_target = Convert.ToInt32(st2);

                Console.WriteLine("Insert desired time to stay in milliseconds");
                st2 = Console.ReadLine();
                time_stay = Convert.ToInt32(st2);

                //corresponds to the actual position set a few lines back, but in a "normalized" form
                x_actual = 42;
                y_actual = 34;

                //the while that checks if the player is near the ball. it turns in the direction of the ball and dashes toward it until it is near (under 3 meters).
                while (((Math.Abs(Math.Abs(x_actual) - Math.Abs(x_target))) > 3) || ((Math.Abs(Math.Abs(y_actual) - Math.Abs(y_target))) > 3))
                {
                    receiveBytes = client.Receive(ref ep);
                    returnData = Encoding.ASCII.GetString(receiveBytes);
                    Thread.Sleep(200);


                    //we read the received string
                    // Split string on spaces.
                    // ... This will separate all the words.
                    string[] words = returnData.Split(delimiterChars, StringSplitOptions.None);
                    foreach (string word in words)
                    {
                        Console.WriteLine(word);
                    }
                    flagInd = 0;//Simulate parameter restart
                    CountAr = words.Length;

                    //The BIG SWITCH. Interprets the received string. could be shortened by calling a method for each case, but since each case 
                    //assigned a different position for each flag, we left it as is
                    switch (words[vari])
                    {
                        //make sure it's not the same see I saw before and that it really is a see so that I only work with see's and try to be more sincronized
                        case "(see":
                            vari = vari + 1;
                            try
                            {
                                time = double.Parse(words[vari]);
                            }
                            catch (FormatException e)
                            {
                                Console.WriteLine(e.Message);
                            }

                            while (vari < CountAr - 1)
                            {
                                vari = vari + 1;
                                if (vari < CountAr - 1)
                                {
                                    string[] number0 = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                    if (!String.IsNullOrEmpty(number0[0]))
                                    {
                                        if (Char.IsNumber(number0[0], 0))
                                        {
                                            check = 0;
                                            while (check == 0 && (vari < CountAr - 1))
                                            {
                                                string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                if (!String.IsNullOrEmpty(number[0]))
                                                {
                                                    if (!Char.IsNumber(number[0], 0))
                                                        check = 1;
                                                }
                                                else
                                                {
                                                    //I used vari = vari + 1 in both cases because of an error where it was used, before it became a problem
                                                    if (!Char.IsNumber(number[1], 0))
                                                        check = 1;
                                                }

                                                vari = vari + 1;
                                            }
                                            if (check == 1)
                                            {
                                                vari = vari - 1;
                                            }

                                        }
                                    }
                                }
                                switch (words[vari])
                                {
                                    case "((p":
                                        //((p "Team1" 1) 9 0 0 0 0 0))
                                        vari = vari + 1;
                                        string number10 = words[vari].Split(new char[] { '"', '"' })[1];
                                        playerTeamName[playerInd] = number10;
                                        vari = vari + 1;
                                        string[] number11 = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                        playerNumb[playerInd] = double.Parse(number11[0]);
                                        vari = vari + 1;
                                        playerDist[playerInd] = double.Parse(words[vari]);
                                        vari = vari + 1;
                                        playerDirec[playerInd] = double.Parse(words[vari]);
                                        vari = vari + 1;
                                        PlayerDistChang[playerInd] = double.Parse(words[vari]);
                                        vari = vari + 1;
                                        PlayerDistChang[playerInd] = double.Parse(words[vari]);
                                        vari = vari + 1;
                                        string[] number1 = words[vari].Split(delimiterplayer, StringSplitOptions.None);
                                        playerBodyFacingDir[playerInd] = double.Parse(number1[0]);

                                        playerInd = playerInd + 1;
                                        break;

                                    case "((b)":
                                        try
                                        {
                                            vari = vari + 1;
                                            bDist = double.Parse(words[vari]);
                                            vari = vari + 1;
                                            bDirect = double.Parse(words[vari]);
                                            vari = vari + 1;
                                            bDistChang = double.Parse(words[vari]);
                                            vari = vari + 1;
                                            string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                            bDirChang = double.Parse(number[0]);


                                        }
                                        catch (FormatException e)
                                        {
                                            Console.WriteLine(e.Message);
                                        }
                                        break;
                                    case "((g":
                                        vari = vari + 1;
                                        switch (words[vari])
                                        {
                                            case "l)":
                                                //g l
                                                try
                                                {
                                                    vari = vari + 1;
                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                    vari = vari + 1;
                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                    xflag[flagInd] = -52.0;
                                                    yflag[flagInd] = 0;
                                                    flagInd = flagInd + 1;

                                                }
                                                catch (FormatException e)
                                                {
                                                    Console.WriteLine(e.Message);
                                                }
                                                break;
                                            case "r)":
                                                //g r
                                                try
                                                {
                                                    vari = vari + 1;
                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                    vari = vari + 1;
                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                    xflag[flagInd] = 52.0;
                                                    yflag[flagInd] = 0;
                                                    flagInd = flagInd + 1;

                                                }
                                                catch (FormatException e)
                                                {
                                                    Console.WriteLine(e.Message);
                                                }
                                                break;
                                        }
                                        break;
                                    case "((f":
                                        vari = vari + 1;
                                        switch (words[vari])
                                        {
                                            case "c)":
                                                // f c
                                                try
                                                {
                                                    vari = vari + 1;
                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                    vari = vari + 1;
                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                    xflag[flagInd] = 0;
                                                    yflag[flagInd] = 0;
                                                    flagInd = flagInd + 1;

                                                }
                                                catch (FormatException e)
                                                {
                                                    Console.WriteLine(e.Message);
                                                }
                                                break;
                                            case "p":
                                                vari = vari + 1;
                                                switch (words[vari])
                                                {
                                                    case "l":
                                                        vari = vari + 1;
                                                        switch (words[vari])
                                                        {
                                                            case "t)":
                                                                // f p l t
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -36;
                                                                    yflag[flagInd] = -20;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "c)":
                                                                //f p l c
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -36;
                                                                    yflag[flagInd] = 0;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "b)":
                                                                //f p l b
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -36;
                                                                    yflag[flagInd] = 20;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                    case "r":
                                                        vari = vari + 1;
                                                        switch (words[vari])
                                                        {
                                                            case "t)":
                                                                //f p r t
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 36;
                                                                    yflag[flagInd] = -20;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "c)":
                                                                //f p r c
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 36;
                                                                    yflag[flagInd] = 0;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "b)":
                                                                //f p r b
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 36;
                                                                    yflag[flagInd] = 20;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                }
                                                break;
                                            case "g":
                                                vari = vari + 1;
                                                switch (words[vari])
                                                {
                                                    case "l":
                                                        vari = vari + 1;
                                                        switch (words[vari])
                                                        {
                                                            case "t)":
                                                                //f g l t
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -52.5;
                                                                    yflag[flagInd] = -7;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "b)":
                                                                //f g l b
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -52.5;
                                                                    yflag[flagInd] = 7;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                    case "r":
                                                        vari = vari + 1;
                                                        switch (words[vari])
                                                        {
                                                            case "t)":
                                                                //f g r t
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 52.5;
                                                                    yflag[flagInd] = -7;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "b)":
                                                                //f g r b
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 52.5;
                                                                    yflag[flagInd] = 7;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                }
                                                break;
                                            case "l":
                                                vari = vari + 1;
                                                switch (words[vari])
                                                {
                                                    case "t)":
                                                        //f l t
                                                        try
                                                        {
                                                            vari = vari + 1;
                                                            FlagDist[flagInd] = double.Parse(words[vari]);
                                                            vari = vari + 1;
                                                            string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                            FlagDirect[flagInd] = double.Parse(number[0]);
                                                            xflag[flagInd] = -52.5;
                                                            yflag[flagInd] = -34;
                                                            flagInd = flagInd + 1;

                                                        }
                                                        catch (FormatException e)
                                                        {
                                                            Console.WriteLine(e.Message);
                                                        }
                                                        break;
                                                    case "b)":
                                                        //f l b Esquina abajo izquierda
                                                        try
                                                        {
                                                            vari = vari + 1;
                                                            FlagDist[flagInd] = double.Parse(words[vari]);
                                                            vari = vari + 1;
                                                            string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                            FlagDirect[flagInd] = double.Parse(number[0]);
                                                            xflag[flagInd] = -52.5;
                                                            yflag[flagInd] = 34;
                                                            flagInd = flagInd + 1;

                                                        }
                                                        catch (FormatException e)
                                                        {
                                                            Console.WriteLine(e.Message);
                                                        }
                                                        break;
                                                    case "0)":
                                                        //f l 0
                                                        try
                                                        {
                                                            vari = vari + 1;
                                                            FlagDist[flagInd] = double.Parse(words[vari]);
                                                            vari = vari + 1;
                                                            string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                            FlagDirect[flagInd] = double.Parse(number[0]);
                                                            xflag[flagInd] = -57.5;
                                                            yflag[flagInd] = 0;
                                                            flagInd = flagInd + 1;

                                                        }
                                                        catch (FormatException e)
                                                        {
                                                            Console.WriteLine(e.Message);
                                                        }
                                                        break;
                                                    case "t":
                                                        vari = vari + 1;
                                                        switch (words[vari])
                                                        {
                                                            case "10)":
                                                                //f l t 10
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -57.5;
                                                                    yflag[flagInd] = -10;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "20)":
                                                                //f l t 20
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -57.5;
                                                                    yflag[flagInd] = -20;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "30)":
                                                                //f l t 30
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -57.5;
                                                                    yflag[flagInd] = -30;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                    case "b":
                                                        vari = vari + 1;
                                                        switch (words[vari])
                                                        {
                                                            case "10)":
                                                                //f l b 10
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -57.5;
                                                                    yflag[flagInd] = 10;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;

                                                            case "20)":
                                                                //f l b 20
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -57.5;
                                                                    yflag[flagInd] = 20;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "30)":
                                                                //f l b 30
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -57.5;
                                                                    yflag[flagInd] = 30;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                }
                                                break;
                                            case "r":
                                                vari = vari + 1;
                                                switch (words[vari])
                                                {
                                                    case "t)":
                                                        //f r t
                                                        try
                                                        {
                                                            vari = vari + 1;
                                                            FlagDist[flagInd] = double.Parse(words[vari]);
                                                            vari = vari + 1;
                                                            string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                            FlagDirect[flagInd] = double.Parse(number[0]);
                                                            xflag[flagInd] = 52.5;
                                                            yflag[flagInd] = -34;
                                                            flagInd = flagInd + 1;

                                                        }
                                                        catch (FormatException e)
                                                        {
                                                            Console.WriteLine(e.Message);
                                                        }
                                                        break;
                                                    case "b)":
                                                        //f r b
                                                        try
                                                        {
                                                            vari = vari + 1;
                                                            FlagDist[flagInd] = double.Parse(words[vari]);
                                                            vari = vari + 1;
                                                            string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                            FlagDirect[flagInd] = double.Parse(number[0]);
                                                            xflag[flagInd] = 52.5;
                                                            yflag[flagInd] = 34;
                                                            flagInd = flagInd + 1;

                                                        }
                                                        catch (FormatException e)
                                                        {
                                                            Console.WriteLine(e.Message);
                                                        }
                                                        break;
                                                    case "0)":
                                                        //f r 0
                                                        try
                                                        {
                                                            vari = vari + 1;
                                                            FlagDist[flagInd] = double.Parse(words[vari]);
                                                            vari = vari + 1;
                                                            string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                            FlagDirect[flagInd] = double.Parse(number[0]);
                                                            xflag[flagInd] = 57.5;
                                                            yflag[flagInd] = 0;
                                                            flagInd = flagInd + 1;

                                                        }
                                                        catch (FormatException e)
                                                        {
                                                            Console.WriteLine(e.Message);
                                                        }
                                                        break;
                                                    case "t":
                                                        vari = vari + 1;
                                                        switch (words[vari])
                                                        {
                                                            case "10)":
                                                                //f r t 10
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 57.5;
                                                                    yflag[flagInd] = -10;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "20)":
                                                                //f r t 20
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 57.5;
                                                                    yflag[flagInd] = -20;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "30)":
                                                                //f r t 30
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 57.5;
                                                                    yflag[flagInd] = -30;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                    case "b":
                                                        vari = vari + 1;
                                                        switch (words[vari])
                                                        {
                                                            case "10)":
                                                                //f r b 10
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 57.5;
                                                                    yflag[flagInd] = 10;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "20)":
                                                                //f r b 20
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 57.5;
                                                                    yflag[flagInd] = 20;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "30)":
                                                                //f r b 30
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 57.5;
                                                                    yflag[flagInd] = 30;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                }
                                                break;
                                            case "c":
                                                vari = vari + 1;
                                                switch (words[vari])
                                                {
                                                    case "t)":
                                                        //f c t
                                                        try
                                                        {
                                                            vari = vari + 1;
                                                            FlagDist[flagInd] = double.Parse(words[vari]);
                                                            vari = vari + 1;
                                                            string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                            FlagDirect[flagInd] = double.Parse(number[0]);
                                                            xflag[flagInd] = 0;
                                                            yflag[flagInd] = -34;
                                                            flagInd = flagInd + 1;

                                                        }
                                                        catch (FormatException e)
                                                        {
                                                            Console.WriteLine(e.Message);
                                                        }
                                                        break;
                                                    case "b)":
                                                        //f c b
                                                        try
                                                        {
                                                            vari = vari + 1;
                                                            FlagDist[flagInd] = double.Parse(words[vari]);
                                                            vari = vari + 1;
                                                            string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                            FlagDirect[flagInd] = double.Parse(number[0]);
                                                            xflag[flagInd] = 0;
                                                            yflag[flagInd] = -34;
                                                            flagInd = flagInd + 1;

                                                        }
                                                        catch (FormatException e)
                                                        {
                                                            Console.WriteLine(e.Message);
                                                        }
                                                        break;
                                                }
                                                break;
                                            case "t":
                                                vari = vari + 1;
                                                switch (words[vari])
                                                {
                                                    case "0)":
                                                        //f t 0
                                                        try
                                                        {
                                                            vari = vari + 1;
                                                            FlagDist[flagInd] = double.Parse(words[vari]);
                                                            vari = vari + 1;
                                                            string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                            FlagDirect[flagInd] = double.Parse(number[0]);
                                                            xflag[flagInd] = 0;
                                                            yflag[flagInd] = -39;
                                                            flagInd = flagInd + 1;

                                                        }
                                                        catch (FormatException e)
                                                        {
                                                            Console.WriteLine(e.Message);
                                                        }
                                                        break;
                                                    case "l":
                                                        vari = vari + 1;
                                                        switch (words[vari])
                                                        {
                                                            case "10)":
                                                                //f t l 10
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -10;
                                                                    yflag[flagInd] = -39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "20)":
                                                                //f t l 20
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -20;
                                                                    yflag[flagInd] = -39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "30)":
                                                                //f t l 30
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -30;
                                                                    yflag[flagInd] = -39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "40)":
                                                                //f t l 40
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -40;
                                                                    yflag[flagInd] = -39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "50)":
                                                                //f t l 50
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -50;
                                                                    yflag[flagInd] = -39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                    case "r":
                                                        vari = vari + 1;
                                                        switch (words[vari])
                                                        {
                                                            case "10)":
                                                                //f t r 10
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 10;
                                                                    yflag[flagInd] = -39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "20)":
                                                                //f t r 20
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 20;
                                                                    yflag[flagInd] = -39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "30)":
                                                                //f t r 30
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 30;
                                                                    yflag[flagInd] = -39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "40)":
                                                                //f t r 40
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 40;
                                                                    yflag[flagInd] = -39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "50)":
                                                                //f t r 50
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 50;
                                                                    yflag[flagInd] = -39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                }
                                                break;
                                            case "b":
                                                vari = vari + 1;
                                                switch (words[vari])
                                                {
                                                    case "0)":
                                                        //f b 0
                                                        try
                                                        {
                                                            vari = vari + 1;
                                                            FlagDist[flagInd] = double.Parse(words[vari]);
                                                            vari = vari + 1;
                                                            string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                            FlagDirect[flagInd] = double.Parse(number[0]);
                                                            xflag[flagInd] = 0;
                                                            yflag[flagInd] = 39;
                                                            flagInd = flagInd + 1;

                                                        }
                                                        catch (FormatException e)
                                                        {
                                                            Console.WriteLine(e.Message);
                                                        }
                                                        break;
                                                    case "l":
                                                        vari = vari + 1;
                                                        switch (words[vari])
                                                        {
                                                            case "10)":
                                                                //f b l 10
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -10;
                                                                    yflag[flagInd] = 39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "20)":
                                                                //f b l 20
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -20;
                                                                    yflag[flagInd] = 39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "30)":
                                                                //f b l 30
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    vari = vari + 1;
                                                                    FlagDirect[flagInd] = double.Parse(words[vari]);
                                                                    xflag[flagInd] = -30;
                                                                    yflag[flagInd] = 39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "40)":
                                                                //f b l 40
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -40;
                                                                    yflag[flagInd] = 39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "50)":
                                                                //f b l 50
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = -50;
                                                                    yflag[flagInd] = 39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                    case "r":
                                                        vari = vari + 1;
                                                        switch (words[vari])
                                                        {
                                                            case "10)":
                                                                //f b r 10
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 10;
                                                                    yflag[flagInd] = 39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "20)":
                                                                //f b r 20
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 20;
                                                                    yflag[flagInd] = 39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "30)":
                                                                //f b r 30
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 30;
                                                                    yflag[flagInd] = 39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "40)":
                                                                //f b r 40
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 40;
                                                                    yflag[flagInd] = 39;
                                                                    flagInd = flagInd + 1;

                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                            case "50)":
                                                                //f b r 50
                                                                try
                                                                {
                                                                    vari = vari + 1;
                                                                    FlagDist[flagInd] = double.Parse(words[vari]);
                                                                    vari = vari + 1;
                                                                    string[] number = words[vari].Split(delimiterNumb, StringSplitOptions.None);
                                                                    FlagDirect[flagInd] = double.Parse(number[0]);
                                                                    xflag[flagInd] = 50;
                                                                    yflag[flagInd] = 39;
                                                                    flagInd = flagInd + 1;
                                                                    number = new string[] { };
                                                                }
                                                                catch (FormatException e)
                                                                {
                                                                    Console.WriteLine(e.Message);
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                }
                                                break;
                                        }

                                        break;
                                    //case "(l":
                                    //switch()
                                    // {

                                    // }
                                    //break;
                                    //case "(B":
                                    //case "(F":
                                    //case "(G":
                                    //case "(P":
                                }
                            }

                            break;
                        case "((sense_body":
                            vari = vari + 1;
                            time = float.Parse(words[vari]);
                            do
                            {
                                vari = vari + 1;
                                switch (words[vari])
                                {
                                    case "(view_mode":
                                        vari = vari + 1;
                                        break;
                                    case "(stamina":
                                        vari = vari + 1;
                                        break;
                                    case "(speed":
                                        vari = vari + 1;
                                        break;
                                    case "(head_angle":
                                        vari = vari + 1;
                                        break;
                                    case "(kick":
                                        vari = vari + 1;
                                        break;
                                    case "(dash":
                                        vari = vari + 1;
                                        break;
                                    case "(turn":
                                        vari = vari + 1;
                                        break;
                                    case "(say":
                                        vari = vari + 1;
                                        break;
                                    case "(turn_neck":
                                        vari = vari + 1;
                                        break;
                                    case "(catch":
                                        vari = vari + 1;
                                        break;
                                    case "(move":
                                        vari = vari + 1;
                                        break;
                                    case "(change_view":
                                        vari = vari + 1;
                                        break;
                                    case "(arm":
                                        vari = vari + 1;
                                        switch (words[vari])
                                        {
                                            case "(movable":
                                                vari = vari + 1;
                                                break;
                                            case "(expires":
                                                vari = vari + 1;
                                                break;
                                            case "(target":
                                                vari = vari + 1;
                                                break;
                                            case "(count":
                                                vari = vari + 1;
                                                break;
                                        }
                                        break;
                                    case "(focus":
                                        vari = vari + 1;
                                        switch (words[vari])
                                        {
                                            case "(target":
                                                vari = vari + 1;
                                                break;
                                            case "(count":
                                                vari = vari + 1;
                                                break;
                                        }
                                        break;
                                    case "(tackle":
                                        vari = vari + 1;
                                        switch (words[vari])
                                        {
                                            case "(expires":
                                                vari = vari + 1;
                                                break;
                                            case "(count":
                                                vari = vari + 1;
                                                break;
                                        }
                                        break;
                                    case "(collision":
                                        vari = vari + 1;
                                        break;
                                    case "(foul":
                                        vari = vari + 1;
                                        break;
                                    case "(charge":
                                        vari = vari + 1;
                                        break;
                                    case "(card":
                                        vari = vari + 1;
                                        break;
                                }
                            }
                            while (vari < CountAr - 1);
                            break;
                        case "(hear":
                            Console.WriteLine("We're doing Hear:\n");
                            break;
                        default:
                            Console.WriteLine("Invalid selection.");
                            break;
                    }
                    //end of big switch

                    vari = 0;
                    //Test to check if I can find player location 
                    if (!(FlagDist == null || FlagDist.Length == 0))
                    {
                        double[] SmallestFlags = new double[flagInd];
                        for (int j = 0; j < Math.Min(flagInd, FlagDist.Length); j++)
                        {
                            SmallestFlags[j] = FlagDist[j];//Takes values over 0
                        }
                        if (!(SmallestFlags == null || SmallestFlags.Length == 0))
                        {
                            double min = SmallestFlags.Min(); //takes the closest
                            int index = Array.IndexOf(SmallestFlags, min);//checks the index, use it to find X and Y of the player
                            Yplayer = yflag[index] - (min * Math.Sin((FlagDirect[index] * Math.PI / 180)));
                            Xplayer = xflag[index] - (min * Math.Cos((FlagDirect[index] * Math.PI / 180)));
                        }
                    }


                    //now I know where the player is
                    //I have two options:

                    // 1)I can go where I said I wanted to go

                    //"normalize" player position to a positive range and changing the Y axis. this is to make formulas more intuitive and minimize 
                    //errors while positioning players. always imagine the field as a first quadrant of a Cartesian plane
                    x_actual = Xplayer + 52;
                    y_actual = ((Yplayer) * (-1)) + 34;

                    x_var = Math.Abs(x_actual - x_target);
                    y_var = Math.Abs(y_actual - y_target);

                    Console.WriteLine("x var is " + x_var);
                    Console.WriteLine("y var is " + y_var);


                    if (x_var == 0)
                    {
                        if (y_target > y_actual)
                        {
                            deg_target = 90;
                        }
                        else
                        {
                            //y_target < y_actual
                            deg_target = 270;
                        }
                    }
                    else if (y_var == 0)
                    {
                        if (x_target > x_actual)
                        {
                            deg_target = 0;
                        }
                        else
                        {
                            //x_target < x_actual
                            deg_target = 180;
                        }
                    }
                    else if ((x_actual > x_target) && (y_actual > y_target))
                    {
                        deg_target = (180 + (90 - (Math.Atan(x_var / y_var) * (180.0 / Math.PI))));
                    }
                    else if ((x_actual < x_target) && (y_actual > y_target))
                    {
                        deg_target = 360 - (90 - (Math.Atan(x_var / y_var) * (180.0 / Math.PI)));
                    }
                    else if ((x_actual > x_target) && (y_actual < y_target))
                    {
                        deg_target = (180 - (90 - (Math.Atan(x_var / y_var) * (180.0 / Math.PI))));
                    }
                    else
                    {
                        //((x_actual < x_target) && (y_actual < y_target))
                        deg_target = (90 - (Math.Atan(x_var / y_var) * (180.0 / Math.PI)));
                    }

                    //given as absolute values. degree 0 is like facing right.
                    Console.WriteLine("deg actual is " + deg_actual);
                    Console.WriteLine("deg target is " + deg_target);

                    //compares both degrees and returns the shortest way to get there; if it's right or left is determined by the corresponding sign
                    turn = Turn_Comp(turn_l, turn_r, deg_actual, deg_target);

                    command = "(turn " + turn + ")";    //if turn is positive, turn right. if negative, turn left
                    b2 = System.Text.Encoding.UTF8.GetBytes(command);
                    client.Send(b2, b2.Length, ep);

                    deg_actual = deg_target;

                    System.Threading.Thread.Sleep(20);

                    command = "(dash 98)";
                    b2 = System.Text.Encoding.UTF8.GetBytes(command);
                    client.Send(b2, b2.Length, ep);
                    //end of part where I go where I wanted to




                    //2) I can go to the ball and kick it. I'm assuming I am already seeing the ball and the goal to the right
                    //can be used to chase player with a few changes
                    /*
                     * 
                if ((Math.Abs(bDist) < 1) && (bDist != 0))
                {
                     * //if I'm near the ball
                    int ind = 0;
                     * //find the goal
                    while ((xflag[ind] != 52) || (yflag[ind] != 0))
                    {
                        //ind = ind + 1;
                    }

                    command = "(kick 90 " + FlagDirect[ind] + ")";//and kick the ball in its direction. assuming I'm seeing it
                    command = "(kick 90 0)";
                    b2 = System.Text.Encoding.UTF8.GetBytes(command);
                    client.Send(b2, b2.Length, ep);

                    ind = 0;

                    Thread.Sleep(10);
                }
                else if (bDist == 0)
                {
                    //what do I do if I'm not doing the ball?
                    //turn till I can see it
                    
                    //but I'm assuming I can see it

                }
                else
                
                     * //if I see the ball turn to it
                    command = "(turn " + bDirect + ")";
                    b2 = System.Text.Encoding.UTF8.GetBytes(command);
                    client.Send(b2, b2.Length, ep);

                    Thread.Sleep(10);

                    command = "(dash 40)"; //dash with dash power from 0 to a 100 real quick
                    b2 = System.Text.Encoding.UTF8.GetBytes(command); 
                    client.Send(b2, b2.Length, ep);

                    Thread.Sleep(10);
                }
                    */



                    words = new string[] { };
                    //restart parameters for a new receive
                    vari = 0; CountAr = 0; i = 0; flagInd = 0; check = 0; playerInd = 0;
                    time = 0; bDist = 0; bDirect = 0; bDistChang = 0; bDirChang = 0;
                    playerDist = new double[33]; playerDirec = new double[33]; PlayerDistChang = new double[33];
                    PlayerDirectChang = new double[33]; playerNumb = new double[33];
                    playerBodyFacingDir = new double[33]; playerHeadFacingDir = new double[33]; FlagDist = new double[33]; FlagDirect = new double[33];
                    xflag = new double[33]; yflag = new double[33];

                }


                //if we reach the desired place, it gets out of the Big While and:
                Console.WriteLine("now waiting amount of time determined by user");
                System.Threading.Thread.Sleep(time_stay);
                //after that it goes back to the while(true) and asks for a new desired direction


            }

            //never reaches this because it's a while(true)
            Console.WriteLine("the end... for now");
            Console.ReadKey(true);
            //end
        }


        static double Turn_Comp(double turn_l, double turn_r, double deg_actual, double deg_target)
        {

            if (deg_actual < deg_target)
            {
                turn_l = deg_actual - deg_target;
                turn_r = turn_l + 360;
            }
            else
            {
                turn_r = deg_actual - deg_target;
                turn_l = turn_r - 360;
            }
            Console.WriteLine("turn left is " + turn_l);
            Console.WriteLine("turn right is " + turn_r);

            if (Math.Abs(turn_l) < Math.Abs(turn_r))
            {
                Console.WriteLine("turn left by " + turn_l);
                return turn_l;
            }
            else
            {
                Console.WriteLine("turn right by " + turn_r);
                return turn_r;
            }
        }
    }
}

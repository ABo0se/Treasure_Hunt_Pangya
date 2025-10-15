using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Treasure_Hunt_Pangya.MainWindow;

namespace Treasure_Hunt_Pangya;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    static readonly Random rand = new Random();
    public MainWindow()
    {
        InitializeComponent();
        AddTreasures();
    }
    public int TreasureHuntPointsTranslation(bool? natural, float luckpercent, float treasurepoints)
    {
        //Initialize variables
        int items = 0;
        float luck = 1.0f;
        float luckfactor = 0.0f; //Max 5
        ///////////////////////////////////////////////////
        //Prevent undesired values
        if (treasurepoints < 0.0f) treasurepoints = 0.0f;
        else if (treasurepoints > 10000.0f) treasurepoints = 10000.0f;
        if (natural == null) natural = false;
        if (luckpercent < 0.0f) luckpercent = 0.0f;
        ///////////////////////////////////////////////////
        //Bonus
        float luckmultiplier = 1.00f + (luckpercent / 100.0f);
        if (natural == true)
        {
            luck += 83.4f; //Increase min luck by factor of 2, luck multiplier, if natural wind
            luckmultiplier += 0.2f;
        }
        if (treasurepoints > 1000)
        {
            luck += (treasurepoints - 1000);
        }
        luck *= (float)luckmultiplier;
        ///////////////////////////////////////////////////
        //Determine luck factor (max 5)
        if (luck > 10000.0f) luck = 10000.0f;
        luckfactor = (float)Math.Log10(luck);
        ///////////////////////////////////////////////////
        //Determine number of dropped items (max 24)
        if (treasurepoints < 10)
        {
            items = 0;
        }
        else if (treasurepoints >= 10 && treasurepoints < 100)
        {
            items = GetnumberofItems(treasurepoints, 10.0f, 100.0f, 1.00f, 2.00f, luckfactor); //1.25 1.5
        }
        else if (treasurepoints >= 100 && treasurepoints < 200)
        {
            items = GetnumberofItems(treasurepoints, 100.0f, 200.0f, 2.00f, 4.00f, luckfactor); //2.5 3.0
        }
        else if (treasurepoints >= 200 && treasurepoints < 300)
        {
            items = GetnumberofItems(treasurepoints, 200.0f, 300.0f, 3.00f, 5.00f, luckfactor); //3.5 4.0
        }
        else if (treasurepoints >= 300 && treasurepoints < 400)
        {
            items = GetnumberofItems(treasurepoints, 300.0f, 400.0f, 3.00f, 7.00f, luckfactor); //4.0 5.0
        }
        else if (treasurepoints >= 400 && treasurepoints < 500)
        {
            items = GetnumberofItems(treasurepoints, 400.0f, 500.0f, 4.00f, 8.00f, luckfactor); //5.0 6.0
        }
        else if (treasurepoints >= 500 && treasurepoints < 600)
        {
            items = GetnumberofItems(treasurepoints, 500.0f, 600.0f, 4.00f, 10.00f, luckfactor); //5.5 7.0
        }
        else if (treasurepoints >= 600 && treasurepoints < 700)
        {
            items = GetnumberofItems(treasurepoints, 600.0f, 700.0f, 5.00f, 11.00f, luckfactor); //6.5 8.0
        }
        else if (treasurepoints >= 700 && treasurepoints < 800)
        {
            items = GetnumberofItems(treasurepoints, 700.0f, 800.0f, 5.00f, 13.00f, luckfactor); //7.0 9.0
        }
        else if (treasurepoints >= 800 && treasurepoints < 900)
        {
            items = GetnumberofItems(treasurepoints, 800.0f, 900.0f, 6.00f, 14.00f, luckfactor); //8.0 10.0
        }
        else if (treasurepoints >= 900 && treasurepoints < 1000)
        {
            items = GetnumberofItems(treasurepoints, 900.0f, 1000.0f, 6.00f, 18.00f, luckfactor); //9.0 12.0
        }
        else if (treasurepoints >= 1000)
        {
            items = GetnumberofItems(treasurepoints, 1000.0f, 2500.0f, 12.00f, 24.00f, luckfactor); //15.0 18.0
        }
        return items;
    }
    //Method to get number of items in treasure box
    public int GetnumberofItems(float treasurepoint,float minTPoint, float maxTPoint, 
                                float mindrop, float maxdrop, float luckfactor)
    {
        //Decide middle of value
        float midTPoint = (minTPoint + maxTPoint) / 2.00f;
        float midItemdrop = (mindrop + maxdrop) / 2.00f;
        float middleValItemChange = (midItemdrop - mindrop);
        float middleValTPointChange = (midTPoint - minTPoint);

        //Prevent undesired values
        if (maxTPoint < 10) return 0;
        if (minTPoint < 10) return 0;
        if (maxdrop <= 0) return 0;
        if (middleValItemChange <= 0) return 0;
        if (middleValTPointChange < 0) return 0;
        if (treasurepoint < 10) return 0;

        //Re-Value variables.
        float actualMindrop;

        //Renewed mindrop depend on luckfactor
        if (luckfactor <= 2 || luckfactor >= 0)
        {
            actualMindrop = mindrop + (midItemdrop - mindrop) * ((luckfactor / 2.00f));
        }
        else if (luckfactor >= 2)
        {
            actualMindrop = midItemdrop + (midItemdrop - mindrop) * ((luckfactor - 2) / 3.00f);
        }
        else return 0;

        //Determine number of items
        decimal randomDrop = Math.Round(((decimal)rand.NextDouble() * (decimal)(maxdrop - actualMindrop)) + (decimal)actualMindrop, 2);
        decimal remainderDrop = Math.Round((randomDrop % 1m), 1);
        
        //int randomRoundValue = rand.Next(1, 101 - (int)remainderDrop);
       
        // + "\nRoundValue: " + randomRoundValue.ToString()
        if (treasurepoint < 1000)
        {
            decimal reDistTPoint = ((decimal)(treasurepoint - minTPoint) / 100.0m);
            //MessageBox.Show("Random Drop: " + randomDrop.ToString() + "\nReDist TPoint: " + reDistTPoint.ToString());
            if (reDistTPoint + remainderDrop >= 1)
                return (int)Math.Ceiling(randomDrop);
            else
                return (int)Math.Floor(randomDrop);
        }
        else
        {
            //MessageBox.Show("Random Drop: " + randomDrop.ToString());
            if (remainderDrop >= 0.5m)
                return (int)Math.Ceiling(randomDrop);
            else
                return (int)Math.Floor(randomDrop);
        }
    }
    #region Treasure Box Initialization
    //Initialize
    public void AddTreasures()
    {
        //////////////////////////////////////////////////////////////////////////////////
        //Initialize items
        //Pang
        Item StrengthBoost = new Item("Strength Boost", 3);
        Item Tranquillizer = new Item("Tranquillizer", 3);
        Item LuckyPangya = new Item("Lucky Pangya", 3);
        Item SpinMastery = new Item("Spin Mastery", 2);
        Item CurveMastery = new Item("Curve Mastery", 2);
        Item MiracleSign = new Item("Miracle Sign", 2);
        Item PowerStrengthBoost = new Item("Power Strength Boost", 1);
        List<Item> PangItems = new List<Item>() 
        {StrengthBoost, Tranquillizer, LuckyPangya, SpinMastery, CurveMastery, MiracleSign, PowerStrengthBoost};
        /////////////////////////////////////
        //Cookie
        Item AutoCalipper = new Item("Auto Calipper", 2);
        Item SpeedBooster = new Item("Speed Booster", 3);
        Item SafeTee = new Item("Safe Tee", 1);
        Item SlientWind = new Item("Slient Wind", 1);
        Item OblivionFlower = new Item("Oblivion Flower", 1);
        Item DualLuckyPangya = new Item("Dual Lucky Pangya", 1);
        Item DualTranquillizer = new Item("Dual Tranquillizer", 1);
        Item ReplayTape = new Item("Replay Tape", 1);
        Item PowerCalipper = new Item("Power Calipper", 1);
        List<Item> CookieItems = new List<Item>()
        {AutoCalipper, SpeedBooster, SafeTee, SlientWind, OblivionFlower, 
         DualLuckyPangya, DualTranquillizer, ReplayTape, PowerCalipper};
        /////////////////////////////////////
        //Cards
        Item BronzeCard = new Item("Bronze Card Pack", 3);
        Item SilverCard = new Item("Sliver Card Pack", 2);
        Item GoldCard = new Item("Gold Card Pack", 1);
        List<Item> Cards = new List<Item>()
        {BronzeCard, SilverCard, GoldCard};
        /////////////////////////////////////
        //Rare Items
        Item CardRemover = new Item("Card Remover", 1);
        List<Item> Rares = new List<Item>()
        {CardRemover};
        //////////////////////////////////////////////////////////////////////////////////
        //Initialize treasure types
        TreasureType Pang = new TreasureType("Pang", PangItems, 1, 1200); //60%
        TreasureType Cookie = new TreasureType("Cookie", CookieItems, 2, 600); //30%
        TreasureType Card = new TreasureType("Card", Cards, 3, 199); //9.95%
        TreasureType Rare = new TreasureType("Rare", Rares, 4, 1); //0.05%
        List<TreasureType> AllTreasures = new List<TreasureType>()
        {Pang, Cookie, Card, Rare};
        //Initialize treasure boxes
        TreasureBox TreasureBox = new TreasureBox("TreasureBox", AllTreasures);
    }
    #endregion
    //Classes for treasure box
    public class TreasureBox(string Name, List<TreasureType> Treasures)
    {
        string Name;
        List<TreasureType> Treasures;
    }
    public class TreasureType(string Name, List<Item> Items, int TreasureType, int rate)
    {
        string Name;
        List<Item> Items;
        int typeoftreasure; //1 = Common, 2 = Uncommon, 3 = Rare, 4 = Epic, 5 = Legendary
        int rate;
    }
    public class Item(string Name, int Rate)
    {
        string Name;
        int Rate;
    }

    private void Go_Button_Click(object sender, RoutedEventArgs e)
    {
        bool? NaturalWind = Natural_Wind_Checkbox.IsChecked;
        float TreasurePoint = float.Parse(TreasureValueText.Text);
        float Luckmultiplier = float.Parse(LuckValueText.Text);
        //MessageBox.Show(TreasureValueText.Text);
        int TreasureCount = TreasureHuntPointsTranslation(NaturalWind, Luckmultiplier, TreasurePoint);
        TreasureCountText.Content = TreasureCount.ToString();
    }
}
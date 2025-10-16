using System.Net;
using System.Printing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
    #region DefaultVars
    static readonly Random rand = new();
    private readonly List<Item> PangItems = [];
    private readonly List<Item> CookieItems = [];
    private readonly List<Item> CardItems = [];
    private readonly List<Item> RareItems = [];
    private static readonly List<TreasureType> AllTreasures = [];
    private static readonly TreasureBox TreasureBoxDefault = new("TreasureBox", AllTreasures);
    private static Dictionary<Item, (string, int, int)> DropChance = [];
    private static float? luckvalue = null; //Always re-calculate at first run and when luck changes.
    private static (Dictionary<Item, (string, int, int)>, List<Item>, List<int>, int) DropLogic;
    private static Dictionary<Item, float> Droppedinfo = [];
    //private Dictionary<Item, (int, int)> DropChance;
    //private int TotalItemDropChance;
    #endregion
    public MainWindow()
    {
        AddTreasures();
        InitializeComponent();
    }
    #region MainCodeLogic
    public (int, Dictionary<Item,int>, Dictionary<Item, float>?, float) TreasureHuntPointsTranslation(bool? natural, float luckpercent, float treasurepoints)
    {
        //Initialize variables
        int itemsCount = 0;
        float luck = 2.0f;
        float luckfactor; //Max 5
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
            luck += 4.4f; //Increase min luck by factor of 2, luck multiplier, if natural wind
            luckmultiplier *= 1.25f;
        }
        if (treasurepoints > 1000)
        {
            luck += (treasurepoints - 1000)/180.0f;
        }
        luck *= luckmultiplier;
        ///////////////////////////////////////////////////
        //Determine luck factor (max 5)
        if (luck > 64.0f) luck = 64.0f;
        luckfactor = (float)Math.Log2(luck) - 1;
        ///////////////////////////////////////////////////
        //Determine number of dropped items (max 24)
        if (treasurepoints < 10)
        {
            itemsCount = 0;
        }
        else if (treasurepoints >= 10 && treasurepoints < 100)
        {
            itemsCount = GetnumberofItems(treasurepoints, 10.0f, 100.0f, 1.00f, 2.00f, luckfactor); //1.25 1.5
        }
        else if (treasurepoints >= 100 && treasurepoints < 200)
        {
            itemsCount = GetnumberofItems(treasurepoints, 100.0f, 200.0f, 2.00f, 4.00f, luckfactor); //2.5 3.0
        }
        else if (treasurepoints >= 200 && treasurepoints < 300)
        {
            itemsCount = GetnumberofItems(treasurepoints, 200.0f, 300.0f, 3.00f, 5.00f, luckfactor); //3.5 4.0
        }
        else if (treasurepoints >= 300 && treasurepoints < 400)
        {
            itemsCount = GetnumberofItems(treasurepoints, 300.0f, 400.0f, 3.00f, 7.00f, luckfactor); //4.0 5.0
        }
        else if (treasurepoints >= 400 && treasurepoints < 500)
        {
            itemsCount = GetnumberofItems(treasurepoints, 400.0f, 500.0f, 4.00f, 8.00f, luckfactor); //5.0 6.0
        }
        else if (treasurepoints >= 500 && treasurepoints < 600)
        {
            itemsCount = GetnumberofItems(treasurepoints, 500.0f, 600.0f, 4.00f, 10.00f, luckfactor); //5.5 7.0
        }
        else if (treasurepoints >= 600 && treasurepoints < 700)
        {
            itemsCount = GetnumberofItems(treasurepoints, 600.0f, 700.0f, 5.00f, 11.00f, luckfactor); //6.5 8.0
        }
        else if (treasurepoints >= 700 && treasurepoints < 800)
        {
            itemsCount = GetnumberofItems(treasurepoints, 700.0f, 800.0f, 5.00f, 13.00f, luckfactor); //7.0 9.0
        }
        else if (treasurepoints >= 800 && treasurepoints < 900)
        {
            itemsCount = GetnumberofItems(treasurepoints, 800.0f, 900.0f, 6.00f, 14.00f, luckfactor); //8.0 10.0
        }
        else if (treasurepoints >= 900 && treasurepoints < 1000)
        {
            itemsCount = GetnumberofItems(treasurepoints, 900.0f, 1000.0f, 6.00f, 18.00f, luckfactor); //9.0 12.0
        }
        else if (treasurepoints >= 1000)
        {
            itemsCount = GetnumberofItems(treasurepoints, 1000.0f, 2500.0f, 12.00f, 24.00f, luckfactor); //15.0 18.0
        }
        ///////////////////////////////////////////////////
        //Determine Items
        Dictionary<Item, (string, int, int)> SynthesisDropChance = new Dictionary<Item, (string, int, int)>(DropChance);
        if (luckvalue != luckfactor)
        {
            //If script is run for the first time or luck has changed, re-calculate drop logic
            luckvalue = luckfactor;
            DropLogic = DroprateAdjustment(SynthesisDropChance, luckfactor);
            Droppedinfo = DropInfo(DropLogic.Item1, DropLogic.Item2, DropLogic.Item4);
        }

       
        Dictionary<Item, int> DroppedItems = GetItems(DropLogic.Item1, DropLogic.Item2, DropLogic.Item3,
                                                      DropLogic.Item4, itemsCount);
        
        return (itemsCount, DroppedItems, Droppedinfo, luckfactor);
    }
    public (Dictionary<Item, float>, float) RateCalculation(bool? natural, float luckpercent, float treasurepoints)
    {
        //Initialize variables
        float luck = 2.0f;
        float luckfactor; //Max 5
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
            luck += 4.4f; //Increase min luck by factor of 2, luck multiplier, if natural wind
            luckmultiplier *= 1.25f;
        }
        if (treasurepoints > 1000)
        {
            luck += (treasurepoints - 1000) / 180.0f;
        }
        luck *= luckmultiplier;
        ///////////////////////////////////////////////////
        //Determine luck factor (max 5)
        if (luck > 64.0f) luck = 64.0f;
        luckfactor = (float)Math.Log2(luck) - 1;
        ///////////////////////////////////////////////////
        //Determine Rate
        Dictionary<Item, (string, int, int)> SynthesisDropChance = new Dictionary<Item, (string, int, int)>(DropChance);
        if (luckvalue != luckfactor || Droppedinfo == null)
        {
            //If script is run for the first time or luck has changed, re-calculate drop logic
            luckvalue = luckfactor;
            DropLogic = DroprateAdjustment(SynthesisDropChance, luckfactor);
            Droppedinfo = DropInfo(DropLogic.Item1, DropLogic.Item2, DropLogic.Item4);
        }
        return (Droppedinfo, luckfactor);
    }
    #endregion
    #region HelperLogics
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
        if (luckfactor <= 2 && luckfactor >= 0)
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
    public Dictionary<Item, (string name, int rate, int type)> GetDropChance()
    {
        var dropChance = new Dictionary<Item, (string name, int rate, int type)>();

        foreach (TreasureType treasure in TreasureBoxDefault.Treasures)
        {
            int chestWeight = treasure.rate;
            int totalItemWeight = treasure.Items.Sum(i => i.rate);

            int assignedWeight = 0;
            var tempWeights = new Dictionary<Item, int>();

            // First pass: calculate truncated weights
            foreach (Item item in treasure.Items)
            {
                int weight = chestWeight * item.rate / totalItemWeight;
                tempWeights[item] = weight;
                assignedWeight += weight;
            }

            // Calculate remainder and distribute it
            int remainder = chestWeight - assignedWeight;
            if (remainder > 0)
            {
                // Distribute 1 extra weight to the largest-rate items
                foreach (var item in treasure.Items.OrderByDescending(i => i.rate))
                {
                    if (remainder == 0) break;
                    tempWeights[item]++;
                    remainder--;
                }
            }

            // Assign final weights
            foreach (var kvp in tempWeights)
            {
                dropChance[kvp.Key] = (kvp.Key.name, kvp.Value, treasure.Treasuretype);
            }
        }

        return dropChance;
    }



    public (Dictionary<Item, (string, int, int)>, List<Item>, List<int>, int)
    DroprateAdjustment(Dictionary<Item, (string, int, int)> DropChance, float luckfactor)
    {
        int totalWeight = DropChance.Values.Sum(v => v.Item2);
        List<Item> items = new List<Item>();
        List<int> cumulativeWeights = new List<int>();

        float droprateMultiplier = (float)Math.Round(Math.Pow(1.7, luckfactor), 2);

        // Step 1: Calculate bonus for boosted items
        var boostedItems = DropChance.Where(kv => kv.Value.Item3 == 3 || kv.Value.Item3 == 4)
                                     .ToDictionary(kv => kv.Key, kv => kv.Value.Item2);
        var otherItems = DropChance.Where(kv => kv.Value.Item3 != 3 && kv.Value.Item3 != 4)
                                   .ToDictionary(kv => kv.Key, kv => kv.Value.Item2);

        // Step 2: Compute raw bonuses
        var bonusWeights = new Dictionary<Item, int>();
        int totalBonus = 0;
        foreach (var kvp in boostedItems)
        {
            int bonus = (int)(kvp.Value * (droprateMultiplier - 1));
            bonusWeights[kvp.Key] = bonus;
            totalBonus += bonus;
        }

        // Step 3: Assign truncated bonuses, handle remainder
        int assignedBonus = bonusWeights.Values.Sum();
        int remainder = totalBonus - assignedBonus;
        foreach (var key in bonusWeights.OrderByDescending(k => boostedItems[k.Key]).Select(k => k.Key))
        {
            if (remainder == 0) break;
            bonusWeights[key]++;
            remainder--;
        }

        // Step 4: Apply bonuses
        foreach (var kvp in bonusWeights)
        {
            var old = DropChance[kvp.Key];
            DropChance[kvp.Key] = (old.Item1, old.Item2 + kvp.Value, old.Item3);
        }

        // Step 5: Deduct totalBonus from other items proportionally
        int deductableSum = otherItems.Values.Sum();
        var deductions = new Dictionary<Item, int>();
        int deducted = 0;
        foreach (var kvp in otherItems)
        {
            int deduction = kvp.Value * totalBonus / deductableSum;
            deductions[kvp.Key] = deduction;
            deducted += deduction;
        }

        remainder = totalBonus - deducted;
        foreach (var key in otherItems.OrderByDescending(k => k.Value).Select(k => k.Key))
        {
            if (remainder == 0) break;
            deductions[key]++;
            remainder--;
        }

        foreach (var kvp in deductions)
        {
            var old = DropChance[kvp.Key];
            DropChance[kvp.Key] = (old.Item1, old.Item2 - kvp.Value, old.Item3);
        }

        // Step 6: Build cumulative weights
        int runningTotal = 0;
        foreach (var kvp in DropChance)
        {
            runningTotal += kvp.Value.Item2;
            items.Add(kvp.Key);
            cumulativeWeights.Add(runningTotal);
        }
        return (DropChance, items, cumulativeWeights, runningTotal);
    }


    public Dictionary<Item, float> DropInfo(Dictionary<Item, (string name, int rate, int type)> DropChance, 
                                            List<Item> Items, double TotalDropWeight)
    {
        Dictionary<Item, float> dropInfo = new Dictionary<Item, float>(Items.Count);

        foreach (Item item in Items)
        {
            int rate = DropChance[item].rate;
            float percentage = (float)(rate * 100.0 / TotalDropWeight);
            if (percentage >= 1.0f)
                percentage = (float)Math.Round(percentage, 2);
            else
                percentage = (float)Math.Round(percentage, 3);
            dropInfo[item] = percentage;
        }

        return dropInfo;
    }

    public Dictionary<Item, int> GetItems(Dictionary<Item, (string name, int rate, int type)> dropChance,
                                          List<Item> items, List<int> cumulativeWeights, int totalWeight, int nItems)
    {
        var droppedItems = new Dictionary<Item, int>();
        var rand = new Random();

        for (int i = 0; i < nItems; i++)
        {
            int randomNumber = rand.Next(1, totalWeight + 1);

            // Efficient binary search for the item index
            int index = GetItemIndexFromWeight(cumulativeWeights, randomNumber);
            Item item = items[index];

            if (droppedItems.ContainsKey(item))
                droppedItems[item]++;
            else
                droppedItems[item] = 1;
        }

        return droppedItems;
    }
    /// Finds the index of the item corresponding to the given random weight using binary search.

    public int GetItemIndexFromWeight(List<int> cumulativeWeights, int randomNumber)
    {
        int index = cumulativeWeights.BinarySearch(randomNumber);

        // If BinarySearch returns a negative number, bitwise complement (~) gives the index of the next larger element
        if (index < 0)
            index = ~index;

        return index;
    }
    #endregion
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
        PangItems.AddRange(new List<Item>()
        {StrengthBoost, Tranquillizer, LuckyPangya, SpinMastery, CurveMastery, MiracleSign, PowerStrengthBoost});
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
        CookieItems.AddRange(new List<Item>()
        {AutoCalipper, SpeedBooster, SafeTee, SlientWind, OblivionFlower, 
         DualLuckyPangya, DualTranquillizer, ReplayTape, PowerCalipper});
        /////////////////////////////////////
        //Cards
        Item BronzeCard = new Item("Bronze Card Pack", 3);
        Item SilverCard = new Item("Sliver Card Pack", 2);
        Item GoldCard = new Item("Gold Card Pack", 1);
        CardItems.AddRange(new List<Item>()
        {BronzeCard, SilverCard, GoldCard});
        /////////////////////////////////////
        //Rare Items
        Item CardRemover = new Item("Card Remover", 1);
        RareItems.AddRange(new List<Item>()
        {CardRemover});
        //////////////////////////////////////////////////////////////////////////////////
        //Initialize treasure types
        TreasureType Pang = new TreasureType("Pang", PangItems, 1, 60000);
        TreasureType Cookie = new TreasureType("Cookie", CookieItems, 2, 38500);
        TreasureType Card = new TreasureType("Card", CardItems, 3, 1480);
        TreasureType Rare = new TreasureType("Rare", RareItems, 4, 20);
        AllTreasures.AddRange(new List<TreasureType>()
        {Pang, Cookie, Card, Rare});

        //Determine drop chance
        DropChance = GetDropChance();
    }

    //Classes for treasure box
    public record TreasureBox(string Name, List<TreasureType> Treasures);
    public record TreasureType(string Name, List<Item> Items, int Treasuretype, int rate);
    public record Item(string name, int rate);
    #endregion
    #region Form Scripts
    private void Go_Button_Click(object sender, RoutedEventArgs e)
    {
        bool? NaturalWind = Natural_Wind_Checkbox.IsChecked;
        float TreasurePoint, Luckmultiplier;
        bool isTreasureValid = float.TryParse(TreasureValueText.Text, out TreasurePoint);
        bool isLuckValid = float.TryParse(LuckValueText.Text, out Luckmultiplier);
        if (!isTreasureValid || !isLuckValid)
        {
            // Handle invalid input here
            MessageBox.Show("Please enter valid numeric values for treasure and luck.");
            return;
        }
        (int, Dictionary<Item, int>, Dictionary<Item, float>?, float) DropResult = 
            TreasureHuntPointsTranslation(NaturalWind, Luckmultiplier, TreasurePoint);
        /////////////////////////////////////////////////////////////////////////////////
        foreach (var child in DropContentPlaceHolder.Children.OfType<Label>().ToList())
        {
            //Clear previous results
            DropContentPlaceHolder.Children.Remove(child);
        }
        DropContent.Content = "Item drop Detail : ";
        foreach (KeyValuePair<Item, int> item in DropResult.Item2)
        {
            DropContentPlaceHolder.Children.Add(new Label()
            {
                Content = item.Value + "x " + item.Key.name,
                FontSize = 11,
                FontFamily = new FontFamily("Segoe UI Variable Display Semibold"),
                Margin = new Thickness(10, 0, 0, 0)
            });
        }
        TreasureCountText.Content = DropResult.Item1.ToString();
        LuckFactorText.Content = DropResult.Item4.ToString("0.00");
    }
    private void Rate_Detail_Button_Click(object sender, RoutedEventArgs e)
    {
        bool? NaturalWind = Natural_Wind_Checkbox.IsChecked;
        float TreasurePoint, Luckmultiplier;
        //////////////////////////////////////////////////////////////
        DropContent.Content = "Item drop rate Detail : ";
        TreasureCountText.Content = "-";
        bool isTreasureValid = float.TryParse(TreasureValueText.Text, out TreasurePoint);
        bool isLuckValid = float.TryParse(LuckValueText.Text, out Luckmultiplier);
        if (!isTreasureValid || !isLuckValid)
        {
            // Handle invalid input here
            MessageBox.Show("Please enter valid numeric values for treasure and luck.");
            return;
        }
        foreach (var child in DropContentPlaceHolder.Children.OfType<Label>().ToList())
        {
            //Clear previous results
            DropContentPlaceHolder.Children.Remove(child);
        }
        (Dictionary<Item, float>, float) RateCal = RateCalculation(NaturalWind, Luckmultiplier, TreasurePoint);
        Droppedinfo = RateCal.Item1;
        LuckFactorText.Content = RateCal.Item2.ToString("0.00"); ;
        foreach (KeyValuePair<Item, float> item in Droppedinfo)
        {
            DropContentPlaceHolder.Children.Add(new Label()
            {
                Content = item.Key.name + " " + item.Value + "%",
                FontSize = 11,
                FontFamily = new FontFamily("Segoe UI Variable Display Semibold"),
                Margin = new Thickness(10, 0, 0, 0)
            });
        }
    }
    #endregion
}

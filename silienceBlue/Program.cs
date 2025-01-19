using System;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Xml.Linq;
using static System.Math;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Reflection.Emit;

namespace silienceBlue
{

    //主要核心類
    class Program
    {


        static void Main(string[] args)
        {
            GameEngine gameEngine = new GameEngine();

            while (true)
            {
                gameEngine.StartGame();
            }
        }
        //預留LINQ使用
        /* 查找單一元素：FirstOrDefault
         PlayerData specificPlayer = playerList.FirstOrDefault(p => p.ChrName == "角色名稱");

        if (specificPlayer != null)
        {
            Console.WriteLine($"找到角色：{specificPlayer.ChrName}");
        }
        else
        {
        Console.WriteLine("找不到該角色");
        }
         */
    }

    public class GameEngine
    {
        public CreateManager createManager { get; } = new CreateManager();
        public bool playerCreated { get; set; } = false;
        private Dictionary<string, IMenuOption> menuOptions;

        public GameEngine()
        {
            menuOptions = new Dictionary<string, IMenuOption>()
            {
                { "1", new CreatPlyaerOption() },
                {"2", new ExitgameOption() }
            };
        }

        public void StartGame()
        {
            bool battleOver = false;

            while (true)
            {
                Console.WriteLine("主選單\n請輸入以下指令：");
                if (!playerCreated) { Console.WriteLine("1.建立角色"); }
                Console.WriteLine("2.離開遊戲");
                switch (Console.ReadLine())
                {
                    case "1":
                        if (!playerCreated)
                        {
                            menuOptions["1"].Execute(this);
                        }
                        else
                        {
                            Console.WriteLine("角色已存在，請勿重複建立");
                        }
                        break;
                    case "2":
                        menuOptions["2"].Execute(this);
                        break;
                    default:
                        Console.WriteLine("輸入錯誤，請重新輸入");
                        break;

                }
                createManager.CreatEnemy();
                PlayerData nowPlayer = createManager.players[0];
                EnemyData nowEnemy = createManager.enemies[0];
                while (!battleOver)
                {
                    battleOver = RunTurn(nowPlayer, nowEnemy);
                }
                createManager.enemies[0] = null;
                createManager.CreatEnemy();
                battleOver = false;

            }

        }

        public bool RunTurn(PlayerData nowPlayer, EnemyData nowEnemy)
        {
            Random rnd = new Random();
            AttackAction attackAction = new AttackAction();
            while (true)
            {
                Console.WriteLine("回合開始\n請選擇行為");
                Console.WriteLine("1.攻擊");
                Console.WriteLine("2.防禦");
                Console.WriteLine("3.投降");
                switch (Console.ReadLine())
                {
                    case "1":
                        int enemyAction = rnd.Next(0, 1);
                        if (enemyAction == 0) attackAction.execute(nowPlayer, nowEnemy, enemyAction);
                        break;
                    case "2":
                        enemyAction = rnd.Next(0, 1);
                        if (enemyAction == 0) attackAction.execute(nowEnemy, nowPlayer, enemyAction);
                        else Console.WriteLine("雙方都保持防禦姿態，未進行攻擊");
                        break;
                    case "3":
                        Console.WriteLine($"{nowPlayer.ChrName}選擇了投降，{nowEnemy.ChrName}獲勝！\n需重新建立角色");
                        playerCreated = false;
                        return true;
                    default:
                        Console.WriteLine("輸入錯誤，請重新輸入");
                        break;
                }

                if (nowPlayer.Hp <= 0)
                {
                    Console.WriteLine($"{nowPlayer.ChrName}已經倒下，{nowEnemy.ChrName}獲勝！");
                    nowPlayer = null;
                    playerCreated = false;
                    return true;

                }
                if (nowEnemy.Hp <= 0)
                {
                    Console.WriteLine($"{nowEnemy.ChrName}已經倒下，{nowPlayer.ChrName}獲勝！");
                    CalculationFormula.CalculateExp(nowPlayer, nowEnemy);
                    CalculationFormula.CheckPlayerLevel(nowPlayer);

                    nowEnemy = null;
                    return true;
                }

            }

        }



    }

    public interface IMenuOption
    {
        void Execute(GameEngine menu);

    }

    public class CreatPlyaerOption : IMenuOption
    {

        public void Execute(GameEngine menu)
        {

            if (!menu.playerCreated)
            {
                menu.createManager.CreatePlayer();
                menu.playerCreated = true;
            }
            else
            {
                Console.WriteLine("角色已存在，請勿重複建立");

            }
        }
    }

    public class ExitgameOption : IMenuOption
    {

        public void Execute(GameEngine menu)
        {
            Console.WriteLine("感謝您的遊玩，下次再來玩吧！");
            Environment.Exit(0);
        }

    }


    public interface ItrunAction
    {
        void execute(CharDataBase actor, CharDataBase target, int enemyAction);
    }

    public class AttackAction : ItrunAction
    {
        public void execute(CharDataBase actor, CharDataBase target, int enemyAction)
        {
            if (enemyAction == 0)
            {
                Console.WriteLine($"{actor.ChrName}執行攻擊");

                CalculationFormula.CalculateBattleDamageUndefended(actor, target);
            }
            else
            {
                CalculationFormula.CalculateBattleDamageDefended(actor, target);
            }
        }
    }

    class Utility
    {
        public static float GetRandomFloat(float min, float max)
        {
            Random rnd = new Random();
            return (float)(rnd.NextDouble() * (max - min) + min);
        }
    }

    public class CreateManager
    {
        Random Random = new Random();
        Utility Utility = new Utility();
        private List<PlayerData> playerList = new List<PlayerData>();
        public List<PlayerData> players => playerList;
        private List<EnemyData> enemyList = new List<EnemyData>();
        public List<EnemyData> enemies => enemyList;
        public EnemyData CreatEnemy()
        {
            EnemyData enemy = null;

            string chrName = "測試怪";
            float height = Utility.GetRandomFloat(1.57f, 2.2f);
            float weight = Utility.GetRandomFloat(40, 100);
            int age = Random.Next(16, 45);
            int sex = Random.Next(0, 1);
            int Level = 1;
            (float bmi, float bodyFatPercentage, float mucel) = CalculationFormula.PhysicalFitnessCalculation(age, height, weight);
            int hp = CalculationFormula.CalculateChrHp(height, weight, age, sex, Level);
            int speed = CalculationFormula.CalculateSpeed(mucel, bodyFatPercentage, age, sex, Level);
            int damage = CalculationFormula.CalculateBaseDamage(speed, mucel, age, sex, Level);
            int defense = CalculationFormula.CalculateBaseDefense(speed, mucel, age, sex, Level);
            enemy = new EnemyData(chrName, Level, 0, hp, 100, damage, defense, speed, height, weight, age, bmi, bodyFatPercentage, mucel, sex);
            enemyList.Add(enemy);
            Console.WriteLine("角色建立成功！\n" +
            $"角色名稱：{enemy.ChrName}\n" +
            $"角色年齡：{enemy.Age}\n" +
            $"角色性別：{enemy.Sex}\n" +
            $"角色等級：{enemy.Level}\n" +
            $"角色生命值：{enemy.Hp}\n" +
            $"角色攻擊力：{enemy.Damage}\n" +
            $"角色防禦力：{enemy.Defense}\n" +
            $"角色速度：{enemy.Speed}\n" +
            $"角色BMI：{enemy.Bmi}\n" +
            $"角色體脂：{enemy.BodyFatPercentage}");
            return enemy;
        }
        public PlayerData CreatePlayer()
        {
            PlayerData player = null;
            Console.WriteLine("請依照格式建立角色\n角色名稱(字串)),身高(可小數),體重(可小數),年齡(整數),性別(0：男，1：女)");
            string userInput = Console.ReadLine();
            string[] splitUserInput = userInput.Split(',');
            if (splitUserInput.Length != 5)
            {
                Console.WriteLine("格式錯誤，請依照格式建立角色");
                return null;
            }


            try
            {
                string chrName = splitUserInput[0];
                float height = float.Parse(splitUserInput[1]);
                float weight = float.Parse(splitUserInput[2]);
                int age = int.Parse(splitUserInput[3]);
                int sex = int.Parse(splitUserInput[4]);
                int level = 1;
                (float bmi, float bodyFatPercentage, float mucel) = CalculationFormula.PhysicalFitnessCalculation(age, height, weight);
                int hp = CalculationFormula.CalculateChrHp(height, weight, age, sex, level);
                int speed = CalculationFormula.CalculateSpeed(mucel, bodyFatPercentage, age, sex, level);
                int damage = CalculationFormula.CalculateBaseDamage(speed, mucel, age, sex, level);
                int defense = CalculationFormula.CalculateBaseDefense(speed, mucel, age, sex, level);
                player = new PlayerData(chrName, 1, 0, hp, 100, damage, defense, speed, height, weight, age, bmi, bodyFatPercentage, mucel, sex);
                playerList.Add(player);

                if (player != null)
                {
                    Console.WriteLine("角色建立成功！\n" +
                        $"角色名稱：{player.ChrName}\n" +
                        $"角色年齡：{player.Age}\n" +
                        $"角色性別：{player.Sex}\n" +
                        $"角色等級：{player.Level}\n" +
                        $"角色生命值：{player.Hp}\n" +
                        $"角色攻擊力：{player.Damage}\n" +
                        $"角色防禦力：{player.Defense}\n" +
                        $"角色速度：{player.Speed}\n" +
                        $"角色BMI：{player.Bmi}\n" +
                        $"角色體脂：{player.BodyFatPercentage}");

                }
                return player;
            }
            catch (Exception ex)
            {
                Console.WriteLine("數據錯誤，請依照格式建立角色");
                return null;
            }
        }
    }

    //用於處理各式計算公式的類
    class CalculationFormula
    {


        public static float CalculateExp(PlayerData Player, EnemyData Enemy)
        {
            Random rnd = new Random();
            float baseEXP = 50f;
            float expFactor = (float)Enemy.Level / (float)Player.Level;
            float randomFactor = rnd.Next(90, 111) / 100f;

            float expGain =
                baseEXP *
                expFactor *
                randomFactor;

            return expGain;//回傳戰勝獲得經驗值
        }

        //每級經驗值門檻
        public static int GetExpThresholdForLevel(int currentLevel)
        {
            int threshold = (int)(100 * Math.Pow(1.2, currentLevel - 1));

            return threshold;
        }

        public static void CheckPlayerLevel(CharDataBase player)
        {
            int currentLevelNeedsExp = GetExpThresholdForLevel(player.Level);

            if (player.Exp > currentLevelNeedsExp)
            {
                player.Level += 1;
                player.Damage = CalculateBaseDamage(player.Speed, player.Mucel, player.Age, player.Sex, player.Level);
                player.Defense = CalculateBaseDefense(player.Speed, player.Mucel, player.Age, player.Sex, player.Level);
                player.Speed = CalculationFormula.CalculateSpeed(player.Mucel, player.BodyFatPercentage, player.Age, player.Sex, player.Level);
                player.Hp = CalculationFormula.CalculateChrHp(player.Height, player.Weight, player.Age, player.Sex, player.Level);
                Console.WriteLine($"{player.ChrName}升級！升至{player.Level}級！" +
                    $"血量現為{player.Hp}" +
                    $"攻擊力現為{player.Damage}" +
                    $"防禦力現為{player.Defense}" +
                    $"速度現為{player.Speed}");
            }

        }


        public static void CalculateBattleDamageUndefended(CharDataBase Attacker, CharDataBase Defender)
        {
            Random rnd = new Random();
            float k1 = 0.1f; //攻擊力調整系數1，控制攻擊力的整體量級
            float k2 = 0.25f; //攻擊力調整系數2，在血量和等級比率以外，再加一個基礎常數，可避免低等級小怪的攻擊力變得太低
            int damage =
                (int)Math.Round((Attacker.Damage / Defender.Defense) *
                (1 + k1 * Attacker.Level) *
                (1 + k2 * Attacker.Speed) *
                (rnd.Next(90, 111) / 100f));

            Defender.Hp -= damage;
            int AttackerHp = (int)(damage * Utility.GetRandomFloat(0.2F, 0.5F));
            Attacker.Hp -= AttackerHp;
            Console.WriteLine($"{Attacker.ChrName}攻擊{Defender.ChrName}，造成{damage}點傷害，{Defender.ChrName}剩餘{Defender.Hp}點生命值\n{Defender.ChrName}進行反擊，{Attacker.ChrName}受到{AttackerHp}點傷害，{Attacker.ChrName}剩餘{Attacker.Hp}點生命值");
        }

        public static void CalculateBattleDamageDefended(CharDataBase Attacker, CharDataBase Defender)
        {
            Random rnd = new Random();
            float k1 = 0.5f; //攻擊力調整系數1，控制攻擊力的整體量級
            float k2 = 0.5f; //攻擊力調整系數2，在血量和等級比率以外，再加一個基礎常數，可避免低等級小怪的攻擊力變得太低
            int damage =
                (int)((Math.Pow(Attacker.Damage, 2) / Defender.Defense * rnd.Next(80, 121) / 100f) *
                (1 + k1 * Attacker.Level) *
                (1 + k2 * Attacker.Speed) *
                (rnd.Next(90, 111) / 100f));

            Defender.Hp -= damage;
            Console.WriteLine($"{Attacker.ChrName}攻擊{Defender.ChrName}，造成{damage}點傷害，{Defender}剩餘{Defender.Hp}點生命值");
        }

        public static int CalculateBaseDamage(int speed, float mucel, int age, int sex, int Level)
        {
            float baseDamage = 30f;

            float speedFactor = 1f + (speed / 100f);

            float mucelFactor = 1f + (mucel / 10f);
            mucelFactor = Math.Clamp(mucelFactor, 0.8f, 2f);

            float ideaAge = 20f;
            float ageDecay = 0.01f;
            float ageDiff = (age - ideaAge);
            float ageFactor = 1f - (ageDiff * ageDecay);
            ageFactor = Math.Clamp(ageFactor, 0.6f, 1.2f);
            float sexFactor = (sex == 0) ? 0.95f : 1f;

            Random rnd = new Random();
            float randomFactor = rnd.Next(90, 111) / 100f;

            float damage =
                baseDamage
                * speedFactor
                * mucelFactor
                * ageFactor
                * sexFactor
                * randomFactor;
            damage += Level * 3f;
            return (int)damage;
        }
        public static int CalculateBaseDefense(int speed, float mucel, int age, int sex, int Level)
        {
            float baseDefense = 10f;

            float speedFactor = 1f + (speed / 100f) * 0.1F;
            speedFactor = Math.Clamp(speedFactor, 0.8f, 1.2f);

            float mucelFactor = 1f + (mucel / 10f);
            mucelFactor = Math.Clamp(mucelFactor, 0.5f, 2.5f);

            float ideaAge = 20f;
            float ageDecay = 0.01f;
            float ageDiff = (age - ideaAge);
            float ageFactor = 1.0f - (ageDiff * ageDecay);
            ageFactor = Math.Clamp(ageFactor, 0.5f, 0.6f);

            float sexFactor = (sex == 0) ? 1.05f : 1f;

            Random rnd = new Random();
            float randomFactor = rnd.Next(90, 111) / 100f;

            float Defense =
                baseDefense
                * speedFactor
                * mucelFactor
                * ageFactor
                * sexFactor
                * randomFactor;
            Defense += Level * 2f;
            return (int)Defense;
        }
        public static int CalculateChrHp(float height, float weight, int age, int sex, int Level)
        {
            int baseHP = 100;
            float heightFactor = 5f;
            float weightFactor = 7f;
            float sexFactor = (sex == 0) ? 0.1f : 0.90f;
            float Hp = baseHP + (height * heightFactor) + (weight * weightFactor) - (sexFactor * age);
            Hp += Level * 10f;
            return (int)Hp;
        }

        public static (float bmi, float bodyFatPercentage, float mucel) PhysicalFitnessCalculation(int age, float hight, float weight)
        {

            float bodyFatjudgement = (age == 0) ? 16.2f : 5.4f;
            float bmi = weight / (hight * hight);
            float bodyFatPercentage = ((1.2f * bmi) + (0.23f * age)) - bodyFatjudgement;
            float mucel = (weight * (100 - bodyFatPercentage) / 100) / (hight * hight); //體脂率 = 體重（100 %－體脂率）] / 身高（公尺）平方;

            return (bmi, bodyFatPercentage, mucel);
        }

        public static int CalculateSpeed(float mucel, float bodyFatPercentage, int age, int sex, int Level)
        {
            float baseSpeed = 5f;//基礎速度
            float mucelFactor = 1f + (mucel) / 10f * 0.2f;
            mucelFactor = Math.Clamp(mucelFactor, 0.5f, 2f);

            float bodyFatFactor = 1f - (bodyFatPercentage - 10f) / 100f * 0.1f;
            bodyFatFactor = Math.Clamp(bodyFatFactor, 0.5f, 1.2f);

            float ideaAge = 20f;//Base age = 20Years old
            float ageFactor = 1f - ((age - ideaAge) * 0.01f);
            ageFactor = Math.Clamp(ageFactor, 0.6f, 1.2f);

            float sexFactor = (sex == 0) ? 1.05f : 1f;

            Random rnd = new Random();
            float randomFactor = rnd.Next(90, 110) / 100f;

            float speed =
                baseSpeed
                * mucelFactor
                * bodyFatFactor
                * ageFactor
                * sexFactor
                * randomFactor;
            speed += Level * 0.5f;
            return (int)speed;
        }
    }

    //角色資料庫的類
    public abstract class CharDataBase
    {

        protected string chrName;
        protected int level;
        protected float exp;
        protected int hp;
        protected int strength;
        protected int damage;
        protected int defense;
        protected int speed;
        protected float height;
        protected float weight;
        protected int age;
        protected float bmi;
        protected float bodyFatPercentage;
        protected float mucel;
        protected int sex;
        public string ChrName { get => chrName; set => chrName = value; }
        public int Level { get => level; set => level = value; }
        public float Exp { get => exp; set => exp = value; }
        public int Hp { get => hp; set => hp = value; }
        public int Strength { get => strength; set => strength = value; }
        public int Damage { get => damage; set => damage = value; }
        public int Defense { get => defense; set => defense = value; }
        public int Speed { get => speed; set => speed = value; }
        public float Height { get => height; set => height = value; }
        public float Weight { get => weight; set => weight = value; }
        public int Age { get => age; set => age = value; }
        public float Bmi { get => bmi; set => bmi = value; }
        public float BodyFatPercentage { get => bodyFatPercentage; set => bodyFatPercentage = value; }
        public float Mucel { get => mucel; set => mucel = value; }
        public int Sex { get => sex; set => sex = value; }
    }

    public class PlayerData : CharDataBase
    {


        public PlayerData(string name, int lv, float exp, int hp, int strength, int damage, int defense, int speed, float height, float weight, int age, float bmi, float bodyFatPercentage, float mucel, int Sex)
        {
            chrName = name;
            level = lv;
            this.exp = exp;

            this.hp = hp;
            this.strength = strength;
            this.damage = damage;
            this.defense = defense;
            this.speed = speed;

            this.height = height;
            this.weight = weight;
            this.age = age;
            this.bmi = bmi;
            this.bodyFatPercentage = bodyFatPercentage;
            this.mucel = mucel;
            this.sex = Sex;

            this.bmi = bmi;
            this.bodyFatPercentage = bodyFatPercentage;
            this.mucel = mucel;
        }
    }

    public class EnemyData : CharDataBase
    {
        public EnemyData(string name, int lv, float exp, int hp, int strength, int damage, int defense, int speed, float height, float weight, int age, float bmi, float bodyFatPercentage, float mucel, int Sex)
        {
            chrName = name;
            level = lv;
            this.exp = exp;

            this.hp = hp;
            this.strength = strength;
            this.damage = damage;
            this.defense = defense;
            this.speed = speed;

            this.height = height;
            this.weight = weight;
            this.age = age;
            this.bmi = bmi;
            this.bodyFatPercentage = bodyFatPercentage;
            this.mucel = mucel;
            this.sex = Sex;

            this.bmi = bmi;
            this.bodyFatPercentage = bodyFatPercentage;
            this.mucel = mucel;
        }
    }
}
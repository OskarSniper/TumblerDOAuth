using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using DontPanic.TumblrSharp.OAuth;

namespace TumblrDirtyWay
{
    public partial class Form1 : Form
    {
        private static OAuthClient _oAuthClient;
        private static Token _oAuthToken;
        private static TumblrClient _tumblrClient;

        public static Form1 form1;

        private static string consumerKey = null;
        private static string consumerSecret = null;

        public Form1()
        {
            InitializeComponent();
            form1 = this;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Console.WriteLine(@"  _____                _     _        ____   ___    _         _   _     ");
            Console.WriteLine(@" |_   _|   _ _ __ ___ | |__ | |_ __  |  _ \ / _ \  / \  _   _| |_| |__  ");
            Console.WriteLine(@"   | || | | | '_ ` _ \| '_ \| | '__| | | | | | | |/ _ \| | | | __| '_ \ ");
            Console.WriteLine(@"   | || |_| | | | | | | |_) | | |    | |_| | |_| / ___ \ |_| | |_| | | |");
            Console.WriteLine(@"   |_| \__,_|_| |_| |_|_.__/|_|_|    |____/ \___/_/   \_\__,_|\__|_| |_|");
            Console.WriteLine();
            Console.WriteLine("Tumblr Dirty Open Authentification, (c) Sebastian.");
            Console.WriteLine("Tool was made in 3 days, used library -> https://github.com/piedoom/TumblrSharp");
            Console.WriteLine("and our own Callback Server because OAuth is shit!");
            Console.WriteLine();

            /*
            *
            * Starting OAuth Callback Server
            *
            */
            Console.WriteLine("Starting Callback Server");
            HttpServer srv = new MyHttpServer(1337);
            Thread thread = new Thread(new ThreadStart(srv.listen));
            thread.Start();
        }

        public async void GotCallback(string data)
        {
            Console.WriteLine("Get access token");
            Token accessToken = await _oAuthClient.GetAccessTokenAsync(_oAuthToken, data);

            Console.WriteLine("Loading without await...");
            this.Invoke(new Action(() =>
            {
                form1.groupBox1.Enabled = true;
                form1.textBox3.Text = accessToken.Key;
                form1.textBox4.Text = accessToken.Secret;
            }));

            Console.WriteLine("Build client");
            _tumblrClient = new TumblrClientFactory().Create<TumblrClient>(consumerKey, consumerSecret, accessToken);
            UserInfo info = await _tumblrClient.GetUserInfoAsync();
            this.Invoke(new Action(() =>
            {
                form1.groupBox2.Enabled = true;
                textBox5.Text = info.Name;
                textBox6.Text = info.Blogs.Length.ToString();
                textBox7.Text = info.FollowingCount.ToString();
                textBox8.Text = info.LikesCount.ToString();
            }));

            this.Invoke(new Action(() =>
            {
                form1.groupBox3.Enabled = true;
                
                /* add to ComboBox */
                comboBox1.Items.Add("Text");
                comboBox1.Items.Add("Answer");
                comboBox1.Items.Add("Audio");
                comboBox1.Items.Add("Chat");
                comboBox1.Items.Add("Link");
                comboBox1.Items.Add("Photo");
                comboBox1.Items.Add("Quote");
                comboBox1.Items.Add("Video");

                /* Add to PostCreationState */
                comboBox2.Items.Add("Draft");
                comboBox2.Items.Add("Private");
                comboBox2.Items.Add("Published");
                comboBox2.Items.Add("Queue");
                comboBox2.Items.Add("Submission");
            }));
        }

        public async void Authenticate()
        {
            _oAuthClient = new OAuthClient(new DontPanic.TumblrSharp.HmacSha1HashProvider(), consumerKey, consumerSecret);
            _oAuthToken = await _oAuthClient.GetRequestTokenAsync("http://127.0.0.1:1337/");
            Console.WriteLine("Open Webbrowser with authorize, because OAuth is shit!");

            // Dirty auth
            System.Diagnostics.Process.Start("https://www.tumblr.com/oauth/authorize?oauth_token=" + _oAuthToken.Key);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show(this, "ConsumerKey or ConsumerSecret is empty!", "Authentication failed!");
                return;
            }
            Console.WriteLine("Start authentication!");
            consumerKey = textBox1.Text;
            consumerSecret = textBox2.Text;
            Authenticate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IEnumerable<string> tags = null;
            Console.WriteLine(textBox10.Text);
            if (!string.IsNullOrEmpty(textBox10.Text))
            {
                if (textBox10.Text.Contains(","))
                {
                    string[] taggies = textBox10.Text.Split(',');
                    for (int i = 0; i < taggies.Length; i++)
                    {
                        //tags.Append(taggies[i]);
                        Console.WriteLine(taggies[i]);
                    }
                }
                else
                {
                    //tags.Append(textBox10.Text);
                    Console.WriteLine(textBox10.Text);
                }
            }
            switch (comboBox1.Text)
            {
                case "Text":
                    PostText(textBox5.Text, richTextBox1.Text, (!string.IsNullOrEmpty(textBox9.Text) ? textBox9.Text : null), tags, getCreationState(comboBox2.Text));
                    break;
                case "Photo":
                    //PostPhoto()
                    break;
                case "Link":
                    //PostLink();
                    break;
                default:
                    MessageBox.Show(this, "Please select any type, or your current type is not supported!", "Message error");
                    break;
            }
        }

        private PostCreationState getCreationState(string name)
        {
            PostCreationState pcs = PostCreationState.Published;
            switch (name)
            {
                case "Queue":
                    pcs = PostCreationState.Queue;
                    break;
                case "Submission":
                    pcs = PostCreationState.Submission;
                    break;
                case "Draft":
                    pcs = PostCreationState.Draft;
                    break;
                case "Private":
                    pcs = PostCreationState.Private;
                    break;
                case "Published":
                    pcs = PostCreationState.Published;
                    break;
            }
            return pcs;
        }

        public async void PostLink(string name, string url, string title, string description, IEnumerable<string> tags, PostCreationState pcs = PostCreationState.Published)
        {
            //await _tumblrClient.CreatePostAsync(name, PostData.CreateLink(url, title, description, tags, pcs));
        }

        public async void PostPhoto(string name, string title = null, IEnumerable<string> tags = null, PostCreationState pcs = PostCreationState.Published)
        {
            //BinaryFile bf = new BinaryFile();
            //await _tumblrClient.CreatePostAsync(name, PostData.CreatePhoto(bf, title, tags, pcs));
        }

        public async void PostText(string name, string body, string title = null, IEnumerable<string> tags = null, PostCreationState pcs = PostCreationState.Published)
        {
            await _tumblrClient.CreatePostAsync(name, PostData.CreateText(body, title, tags, pcs));
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Changed to " + comboBox1.Text);
        }
    }
}

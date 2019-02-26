using System;
using System.Windows.Forms;
using System.Windows.Controls;


namespace SKELETONTOIMGAE
{
   
    public partial class LogIn : Form
    {
        
        IDandPW[] HOST;
        public bool OPENKINECT = false;
        public string SupervisorName = null;

        // 사운드
        private SoundPlayerAction alam = new SoundPlayerAction();


        public LogIn()
        {
            

            MessageBox.Show(" 관리자 학번과 비밀번호를 입력해주세요 ", "EYE POD");
            InitializeComponent();
            HOST = new IDandPW[6];

            HOST[0] = new IDandPW();
            HOST[0].ID = "201101797"; // 김종원
            HOST[0].PASSWORD = "053205";
            HOST[0].NAME = "김종원 회장";

            HOST[1] = new IDandPW();
            HOST[1].ID = "201101820"; // 전영재
            HOST[1].PASSWORD = "5858";
            HOST[1].NAME = "전영재 기획팀장";

            HOST[2] = new IDandPW();
            HOST[2].ID = "201101812"; // 이병희
            HOST[2].PASSWORD = "7626";
            HOST[2].NAME = "이병희 인사팀장";

            HOST[3] = new IDandPW();
            HOST[3].ID = "201101799"; // 노원규
            HOST[3].PASSWORD = "4063";
            HOST[3].NAME = "노원규 영업팀장";

            HOST[4] = new IDandPW();
            HOST[4].ID = "201101793"; // 김신항
            HOST[4].PASSWORD = "9209";
            HOST[4].NAME = "김신항 홍보팀장";

            HOST[5] = new IDandPW();
            HOST[5].ID = "professorj"; // 조정호 교수님
            HOST[5].PASSWORD = "";
            HOST[5].NAME = "조정호 교수";

        }

        // 로그인 메서드
          
        private void IDbox_Click(object sender, EventArgs e)
        {
            IDbox.Text = "";
        }
        private void PassWordbox_Click(object sender, EventArgs e)
        {
            PassWordbox.Text = "";
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            int HOSTCOUNT = 0;
            for (int i = 0; i < HOST.Length; i++)
            {
                if (IDbox.Text == HOST[i].ID && PassWordbox.Text == HOST[i].PASSWORD && HOSTCOUNT<1)
                {
                    MessageBox.Show("환영합니다." + HOST[i].NAME +"님.");
                    HOSTCOUNT++;
                    OPENKINECT = true;
                    SupervisorName = HOST[i].NAME;
                    Close();
                }
            }
            if (HOSTCOUNT == 0)
            {
                MessageBox.Show("관리자 권한이 없습니다." + IDbox.Text + "님.","문의 010-2731-4186");
                
            }
        }
    }
}

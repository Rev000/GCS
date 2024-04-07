using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCS.TL.Test
{
    public class InfeedConv : ConvBase
    {
        ConvData conv1 = new ConvData();
   
        public InfeedConv(ConvData data) : base(data)
        {
            // 생성자에서는 부모 클래스의 생성자를 호출하여 초기화합니다.
            // 부모 클래스의 생성자에서 ConvData 구조체의 내용을 이미 설정했으므로
            // 추가적인 초기화가 필요하지 않습니다.
        }

        public override void InitSetting(float delayTime, string name, string info, int useCount)
        {
            base.InitSetting(0, "InfeedConveyor", "ss", 0);
        }

        public override void Using()
        {
            base.Using();
        }
    }
}

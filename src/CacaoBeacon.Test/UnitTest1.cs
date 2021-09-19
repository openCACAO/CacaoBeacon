using System;
using System.Collections.Generic;
using Xunit;

#if false
namespace TestProject1
{
    public class UnitTest1
    {

        // CacaoBeaconServer
        // ���M���i�y���t�F�����j
        // 1. 1���̍ŏ���TEKi�𐶐����ĕێ�����
        // 2. TEKi����144����RPIlist���쐬���Ă���
        // 3. �����ɉ����āARPIj �����o���Ĕ��M����
        // 4. ���M����TEKi�������Z�b�g�ŕێ�����

        // CacaoBeaconReceiver
        // ��M���i�Z���g�����j
        // 1. ���M������ RPI ����M�����Ɠ����ɕێ�����B
        // 2. �ŏ���RPI�́A�J�n�����Ƃ��Ă͕ێ�����
        // 3. �A������RPI�́A�Y������RPI�������ďI�������Ƃ��čX�V���ĕێ�����
        // 4. RPI, �J�n����, �I�������Ń����Z�b�g�ɂȂ�


        // CacaoBeaconManager
        // ��M����RPI�̏ƍ��`�F�b�N�i�ڐG�m�F�j
        // 1. TEK�̃��X�g����M����
        // 2. TEKi����ARPIlist�𐶐�����
        // 3. RPIlist �Ɠ����ŕێ����� RPI���ƍ����āA�ڐG�m�F���`�F�b�N����
        // 4. �Y���Ȃ��̏ꍇ�́A�ڐG�m�F�Ȃ�
        // 5. �Y������̏ꍇ�́A�ڐG�m�F����
        // 6. TEKi����metadata�𕜍�����
        // 7. �A������RPI�i�J�n�����A�I�������j�𗘗p���āA�ڐG���Ԃ��m�肷��
        // 8. �ڐG���Ԃɂ��A���X�N�l���m�肷��H

        public class CacaoBeaconServer
        {
            public void InitTEK(byte[] tek = null) { }
            public TEK TEK { get; }
            public List<TEK> TEKs { get; }
            public byte[] PRI { get; }
            public List<byte[]> PRIs { get; }
            public DateTime Today { get; set; }

            public void Send(byte[] rpi = null) { }
        }

        public class TEK
        {
            public byte[] Key { get; }
            public DateTime Date;           // ������
        }

        public class CacaoBeaconReceiver
        {
            public void Recv(byte[] pri, DateTime? time = null) { }
            public List<RPI> RPIs { get; }
        }

        public class RPI
        {
            public byte[] Key { get; }
            public DateTime StartTime { get; } // �J�n����
            public DateTime EndTime { get; }   // �I������
        }

        public class CacaoBeaconManager
        {
            public void Download() { }
            public List<TEK> TEKs { get; }
            public void Detect( List<RPI> pris ) { }
            public List<Result> GetSummary() {
                return null;
            }
        }


        public class Result
        {
            public TEK TEK { get; }
            public DateTime StartTime { get; }
            public DateTime EndTime { get; }
            public TimeSpan Time { get; }   // �ڐG����
            public int Risk { get; }        // ���X�N�l
            public int RSSI { get; }        // �����i�d�g���x�j

        }

        [Fact]
        public void Test1()
        {
            var cbserver = new CacaoBeaconServer();
            // TEK�𐶐�
            cbserver.InitTEK();
            // TEK���m�F
            byte[] tek = cbserver.TEK.Key;
            // �ߋ���TEK�̈ꗗ���擾
            var lst = cbserver.TEKs;
            // �������ł�RPI���擾
            var rpi = cbserver.PRI;
            // ������144��RPI���擾
            var rpis = cbserver.PRIs;
            // �������擾�i�e�X�g�p�ɓ������ύX�ł���j
            var day = cbserver.Today;

            // RPI�𔭐M����
            cbserver.Send();
            // 10���o������ RPI ��؂�ւ���
        }

        [Fact]
        public void Test2()
        {
            var cbreceiver = new CacaoBeaconReceiver();

            // �O����RPI����M����
            byte[] pri = new byte[16];
            cbreceiver.Recv(pri);
            // ��M����RPI�̃��X�g
            var lst = cbreceiver.RPIs;

            // ����RPI����M����ƁA�I���������X�V�����
            cbreceiver.Recv(pri);
            RPI pri0 = cbreceiver.RPIs.Find(t => t.Key == pri);
            DateTime start = pri0.StartTime;
            DateTime end = pri0.EndTime;


        }

        [Fact]
        public void Test3()
        {
            var cbmanager = new CacaoBeaconManager();
            var cbreceiver = new CacaoBeaconReceiver();

            // zip �t�@�C�����_�E�����[�h���� TEK�̃��X�g�𓾂�
            cbmanager.Download();
            var teks = cbmanager.TEKs;

            // ��M����RPI�Əƍ�����
            cbmanager.Detect(cbreceiver.RPIs);
            // �f�f���ʂ𓾂�
            var result = cbmanager.GetSummary();

            foreach ( var it in result )
            {
                var tek = it.TEK;
                var date = it.StartTime;        // �ڐG��������
                var time = it.Time;             // �ڐG����
                var rssi = it.RSSI;             // �ڐG�����i�ł��߂Â��������j
            }

            // 臒l�����āA�ʒm���o�����ǂ������߂�

        }
    }
}
#endif
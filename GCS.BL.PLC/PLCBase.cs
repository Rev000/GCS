using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using S7.Net;


namespace GCS.BL.PLC
{
    abstract public class PLCBase
    {
        private static readonly ILog logger = LogManager.GetLogger("PLCBase");

        CancellationTokenSource cts;

        #region "[Properties]"
        public string PLCName { get; set; }
        protected Plc siemens;
        protected CpuType cpuType { get; set; }
        protected string ipAddress { get; set; }
        protected short rack { get; set; }
        protected short slot { get; set; }
        public List<INFItem> INFItems { get; set; }
        #endregion

        public List<Tuple<string, string, int>> AlarmList = new List<Tuple<string, string, int>>();

        private string ADDR_DBX_IS_WORKING = string.Format("DB450.DBX1.5");
        private string ADDR_DBX_DID_END_WORK = string.Format("DB450.DBX1.6");
        private string ADDR_DBX_COMM_FAULT = string.Format("DB450.DBX1.7");

        private string ADDR_DBW_MES_PLC_LIFE_CHECK = string.Format("DB450.DBW104");

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="cpuType"></param>
        /// <param name="ipAddress"></param>
        /// <param name="rack"></param>
        /// <param name="slot"></param>
        protected PLCBase(CpuType cpuType, string ipAddress, short rack, short slot)
        {
            INFItems = new List<INFItem>();

            this.cpuType = cpuType;
            this.ipAddress = ipAddress;
            this.rack = rack;
            this.slot = slot;
        }

        public Plc GetSiemens()
        {
            return siemens;
        }

        /// <summary>
        /// PLC 연결
        /// </summary>
        /// <returns></returns>
        public ErrorCode Connect()
        {
            ErrorCode errorCode = ErrorCode.NoError;

            if (siemens == null)
                siemens = new Plc(cpuType, ipAddress, rack, slot);
            
            siemens.Open();
            if (siemens.IsConnected == true)
            {
                cts = new CancellationTokenSource();
                CmdLifeCheck(cts.Token);

            }
  
            return errorCode;
        }

        /// <summary>
        /// PLC 연결해제
        /// </summary>
        public void DisConnect()
        {
            cts.Cancel();
            siemens.Close();
        }

        /// <summary>
        /// PLC 연결상태
        /// </summary>
        public bool IsConnected
        {
            get { return siemens.IsConnected; }
        }

        /// <summary>
        /// PLC 데이터 쓰기
        /// </summary>
        /// <param name="address">주소</param>
        /// <param name="obj">값</param>
        /// <returns></returns>
        public ErrorCode Write(string address, object obj)
        {
            ErrorCode errorCode = ErrorCode.NoError;

            try
            {
                if (siemens != null && siemens.IsConnected)
                {
                    lock (lockObject)
                    {
                        errorCode = siemens.Write(address, obj);
                    }
                }
                else
                {
                    errorCode = ErrorCode.ConnectionError;

                    if (TCPUtil.PingCheck(ipAddress)
                        && !cts.Token.IsCancellationRequested
                        && TCPUtil.CanConnect(ipAddress, 102))
                    {
                        while (siemens.Open() != ErrorCode.NoError)
                        {
                            Debug.WriteLine("Reconnection!");
                            Thread.Sleep(1000);
                        }
                    }
                }

                return errorCode;
            }
            catch (Exception ex)
            {
                logger.Debug(ex);
                return ErrorCode.WriteData;
            }
        }

        private object lockObject = new object();
        /// <summary>
        /// PLC 데이터 읽기
        /// </summary>
        /// <param name="address">주소</param>
        /// <returns></returns>
        public object Read(string address)
        {
            object result = new object();

            try
            {
                if (siemens != null && siemens.IsConnected)
                {
                    lock (lockObject)
                    {
                        result = siemens.Read(address);
                    }
                }
                else
                {
                    if (TCPUtil.PingCheck(ipAddress)
                          && !cts.Token.IsCancellationRequested
                          && TCPUtil.CanConnect(ipAddress, 102))
                    {
                        while (siemens.Open() != ErrorCode.NoError)
                        {
                            Debug.WriteLine("Reconnection!");
                            Thread.Sleep(1000);
                        }
                    }
                }

                //Debug.WriteLine(string.Format("address = {0}, result type = {1}", address, result.GetType()));
                return result;
            }
            catch (Exception ex)
            {
                logger.Debug(ex);
                return ErrorCode.ReadData;
            }
        }

        protected Tuple<ErrorCode, string> ReadString(string address, int length)
        {
            string result = string.Empty;

            try
            {
                if (siemens != null && siemens.IsConnected)
                {
                    string[] strings = address.Split(new char[] { '.' });
                    if (strings.Length < 2)
                        throw new Exception();

                    int mDB = int.Parse(strings[0].Substring(2));
                    string dbType = strings[1].Substring(0, 3);
                    int dbIndex = int.Parse(strings[1].Substring(3));

                    byte[] ret = siemens.ReadBytes(DataType.DataBlock, mDB, dbIndex, length);
                    result = Encoding.Default.GetString(ret);
                }
                else
                {
                    if (TCPUtil.PingCheck(ipAddress)
                          && TCPUtil.CanConnect(ipAddress, 102))
                    {
                        while (siemens.Open() != ErrorCode.NoError)
                        {
                            Debug.WriteLine("Reconnection!");
                            Thread.Sleep(1000);
                        }
                    }
                }

                //Debug.WriteLine(string.Format("address = {0}, result type = {1}", address, result.GetType()));
                return new Tuple<ErrorCode, string>(ErrorCode.NoError, result);
            }
            catch (Exception ex)
            {
                logger.Debug(ex);
                return new Tuple<ErrorCode, string>(ErrorCode.ReadData, "");
            }
        }

        public Tuple<ErrorCode, bool> ReadBit(string address)
        {
            bool ret;
            Tuple<ErrorCode, bool> tpl;
            if (bool.TryParse(Read(address).ToString(), out ret))
            {
                tpl = new Tuple<ErrorCode, bool>(ErrorCode.NoError, ret);
            }
            else
            {
                tpl = new Tuple<ErrorCode, bool>(ErrorCode.WrongVarFormat, false);
            }

            return tpl;
        }

        public Tuple<ErrorCode, bool> ReadBit(DataType dataType, int db, int startByteAddr, int bitIndex)
        {
            byte[] bytes = ReadBytes(dataType, db, startByteAddr, 1);
            if (bytes != null && bytes.Length > 0)
            {
                bool ret = bytes[0].SelectBit(bitIndex);
                return new Tuple<ErrorCode, bool>(ErrorCode.NoError, ret);
            }
            else
            {
                return new Tuple<ErrorCode, bool>(ErrorCode.ReadData, false);
            }
        }

        public byte[] ReadBytes(DataType dataType, int db, int startByteAddr, int count)
        {
            return siemens.ReadBytes(dataType, db, startByteAddr, count);
        }




        /// <summary>
        /// 해당 PLC 접속 가능 여부 체크
        /// </summary>
        /// <returns></returns>
        public bool CanConnect()
        {
            return TCPUtil.CanConnect(ipAddress, 102);
        }

        public void CmdLifeCheck(CancellationToken token)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (token != null && token.IsCancellationRequested)
                        break;

                    Thread.Sleep(1000);

                    try
                    {
                        Write(ADDR_DBW_MES_PLC_LIFE_CHECK, "0");
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
            });
        }

        /// <summary>
        /// 현재 작업중 확인
        /// </summary>
        /// <returns></returns>
        public Tuple<ErrorCode, bool> IsWorking()
        {
            //return ReadBit(ADDR_DBX_IS_WORKING);
            return ReadBit(DataType.DataBlock, 450, 1, 5);
        }

        /// <summary>
        /// 작업 종료 여부 확인
        /// </summary>
        /// <returns></returns>
        public Tuple<ErrorCode, bool> DidWorkEnd()
        {
            return ReadBit(ADDR_DBX_DID_END_WORK);
        }

        /// <summary>
        /// Communication Fault
        /// </summary>
        /// <returns></returns>
        public Tuple<ErrorCode, bool> HasCommFault()
        {
            return ReadBit(ADDR_DBX_COMM_FAULT);
        }

        public void AlarmWatcher(CancellationToken token)
        {
            Task.Run(() =>
            {
                if (AlarmList != null && AlarmList.Count > 0)
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        for (int i = 0; i < AlarmList.Count; i++)
                        {
                            try
                            {
                                var result = Read(AlarmList[i].Item2);

                                if (result is ErrorCode && (ErrorCode)result != ErrorCode.NoError)
                                {
                                    throw new Exception(((ErrorCode)result).ToString() + "\n" + "Tag: " + AlarmList[i]);
                                }

                                string alarmBits = ConvertBits(int.Parse(result.ToString()));

                                string errorYN = string.Empty;
                                string errorMessage = string.Empty;

                                CBizPLC.BL_POP_SET_INF_PLC_ALARM(AlarmList[i].Item1, alarmBits, out errorYN, out errorMessage);

                                if (errorYN.Equals("Y"))
                                {
                                    throw new Exception(errorMessage);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex);
                            }

                            Thread.Sleep(500);
                        }

                        Thread.Sleep(1000);
                    }
                }
            });
        }

        public string ConvertBits(int value)
        {
            string ret = Convert.ToString(value, 2);
            ret = ret.PadLeft(16, '0');
            ret = string.Format("{0}{1}", ret.Substring(8, 8), ret.Substring(0, 8));
            char[] arr = ret.ToCharArray();
            Array.Reverse(arr);

            return new string(arr);
        }


        public void BitTransfer(CancellationToken token)
        {
            Task.Run(() =>
            {
                List<string> tags = new List<string>();
                tags.Add("DB450.DBX41.0");
                tags.Add("DB450.DBX41.1");
                tags.Add("DB450.DBX41.2");
                tags.Add("DB450.DBX41.3");
                tags.Add("DB450.DBX41.4");
                tags.Add("DB450.DBX41.5");
                tags.Add("DB450.DBX41.6");
                tags.Add("DB450.DBX41.7");

                tags.Add("DB450.DBX42.0");
                tags.Add("DB450.DBX42.1");
                tags.Add("DB450.DBX42.2");
                tags.Add("DB450.DBX42.3");
                tags.Add("DB450.DBX42.4");
                tags.Add("DB450.DBX42.5");
                tags.Add("DB450.DBX42.6");
                tags.Add("DB450.DBX42.7");

                tags.Add("DB450.DBX43.0");
                tags.Add("DB450.DBX43.1");
                tags.Add("DB450.DBX43.2");
                tags.Add("DB450.DBX43.3");
                tags.Add("DB450.DBX43.4");
                tags.Add("DB450.DBX43.5");
                tags.Add("DB450.DBX43.6");
                tags.Add("DB450.DBX43.7");

                tags.Add("DB450.DBX44.0");
                tags.Add("DB450.DBX44.1");
                tags.Add("DB450.DBX44.2");
                tags.Add("DB450.DBX44.3");
                tags.Add("DB450.DBX44.4");
                tags.Add("DB450.DBX44.5");
                tags.Add("DB450.DBX44.6");
                tags.Add("DB450.DBX44.7");

                while (true)
                {
                    try
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        foreach (string s in tags)
                        {
                            object obj = siemens.Read(s);
                            //Debug.WriteLine("PLC={0} => Address={1}, Value={2}", PLCName, s, obj.ToString());

                            //if (!s.Equals("DB450.DBX0.0") && !s.Equals("DB450.DBX0.1"))
                            Debug.Assert(obj.ToString() == "False", string.Format("{0} > Invalid value={1} of PLC Name is ={2}", s, obj.ToString(), PLCName));
                        }
                        //Debug.WriteLine("Elapsed Time={0}", sw.ElapsedMilliseconds / 1000);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }

                    Thread.Sleep(100);
                }
            });
        }

        //public void BitTransfer(CancellationToken token)
        //{
        //    Task.Run(() =>
        //    {
        //        while (true)
        //        {
        //            if (token.IsCancellationRequested)
        //                break;

        //            try
        //            {
        //                if (INFItems != null && INFItems.Count > 0)
        //                {
        //                    var task = Task.Run(() => 
        //                    {
        //                        for (int i = 0; i < INFItems.Count; i++)
        //                        {
        //                            INFItem item = INFItems[i];
        //                            if (item.Target != null
        //                                && !string.IsNullOrEmpty(item.SourceAddr)
        //                                && !string.IsNullOrEmpty(item.TargetAddr))
        //                            {
        //                                Stopwatch sw = Stopwatch.StartNew();

        //                                Tuple<ErrorCode, bool> ret = ReadBit(item.SourceAddr);
        //                                if (ret.Item1 == ErrorCode.NoError)
        //                                {
        //                                    bool value = ret.Item2;
        //                                    if (item.UseYN.Equals("Y"))
        //                                    {
        //                                        //if (ret.Item2 != item.Target.ReadBit(item.TargetAddr).Item2)
        //                                        //{
        //                                        //    logger.ErrorFormat("Source PLC:{0} - {1}", item.SourceID, item.ToString());
        //                                        //}
        //                                        ErrorCode errorCode = ErrorCode.NoError;

        //                                        if (value != item.LastBit)
        //                                        {
        //                                            if (item.BitReverse.Equals("Y"))
        //                                            {
        //                                                value = !value;
        //                                            }

        //                                            errorCode = item.Target.Write(item.TargetAddr, value);

        //                                            //if (item.AfterReset.Equals("Y"))
        //                                            //{
        //                                            //    Thread.Sleep(1000);
        //                                            //    item.Target.Write(item.TargetAddr, !value);
        //                                            //}

        //                                            //if (item.SourceID.Equals("FILLING"))
        //                                            //{
        //                                            //    logger.ErrorFormat("Source PLC:{0}[{1}]={2}\t Target PLC:{3}[{4}]={5}"
        //                                            //    , item.SourceID
        //                                            //    , item.SourceAddr
        //                                            //    , ret.Item2
        //                                            //    , item.TargetID
        //                                            //    , item.TargetAddr
        //                                            //    , item.Target.ReadBit(item.TargetAddr).Item2);
        //                                            //}
        //                                        }

        //                                        var obj = item.Target.ReadBit(item.TargetAddr);
        //                                        if (obj.Item1 == ErrorCode.NoError)
        //                                        {
        //                                            if (value != obj.Item2)
        //                                            {
        //                                                string log = string.Format("Source PLC:{0}[{1}]={2}\t Target PLC:{3}[{4}]={5} > Thread Id={6} ElasedTime:{7}"
        //                                                    , item.SourceID
        //                                                    , item.SourceAddr
        //                                                    , ret.Item2
        //                                                    , item.TargetID
        //                                                    , item.TargetAddr
        //                                                    , obj.Item2
        //                                                    , Task.CurrentId.Value
        //                                                    , sw.ElapsedMilliseconds / 1000.0);

        //                                                logger.Debug(log);
        //                                            }
        //                                        }

        //                                        Debug.Assert(value == obj.Item2, "Not same source with target!");

        //                                        item.LastBit = value;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    logger.ErrorFormat(item.ToString());
        //                                }

        //                            }
        //                        }
        //                    });

        //                    Thread.Sleep(500);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                logger.Error(ex);
        //            }
        //        }
        //    });
        //}
    }

}

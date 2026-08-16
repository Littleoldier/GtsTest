using System;
using gts;

namespace GtsTest
{
    /// <summary>
    /// 模型层：封装对 gts.mc 的调用，支持模拟模式
    /// </summary>
    public class GtsModel
    {
        // 模拟模式开关
        public static bool UseSimulation { get; set; } = false;

        // 模拟数据存储（按轴号索引，1~8）
        private static double[] _simPos = new double[9];
        private static double[] _simVel = new double[9];
        private static double[] _simAcc = new double[9];
        private static int[] _simStatus = new int[9];
        private static int[] _simMode = new int[9];
        private static bool _simInitialized = false;
        private static uint _simClock = 0;
        // 模拟用变量（新增）
        private static double[] _simTargetPos = new double[9];
        private static bool[] _simIsMoving = new bool[9];

        /// <summary>
        /// 实时检测硬件环境是否就绪
        /// </summary>
        public static bool CheckHardwareAvailable()
        {
            try
            {
                // 调用 GT_GetCardNo 只需要 DLL 存在，不需要打开卡
                short result1 = mc.GT_GetCardNo(out short cardNo);
                if(result1 == 0)
                {
                    return true;
                }
                else
                {
                    return true;
                }
                    // 只要能执行到这里，说明 gts.dll 已加载成功

                    short result = mc.GT_Open(0, 0);
                if (result != 0)
                {
                    // 打开失败，但 DLL 已加载，说明驱动存在但卡可能未连接
                    return false;
                }
                mc.GT_Close(); // 成功打开，立即关闭释放
                return true;
            }
            catch (DllNotFoundException)
            {
                return false;
            }
            catch (BadImageFormatException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void InitializeSimulation()
        {
            if (_simInitialized) return;
            for (int i = 1; i <= 8; i++)
            {
                _simPos[i] = 0;
                _simVel[i] = 0;
                _simAcc[i] = 0;
                _simStatus[i] = 0x200;   // 默认使能
                _simMode[i] = 0;
            }
            _simInitialized = true;
        }

        // ---------- 模拟辅助方法 ----------
        private static short SimulateOpen(short card, short mode)
        {
            InitializeSimulation();
            return 0;
        }

        private static short SimulateReset() => 0;

        private static short SimulateGetSts(short axis, out int status, short count, out uint clk)
        {
            status = (axis >= 1 && axis <= 8) ? _simStatus[axis] : 0;
            clk = _simClock++;
            return 0;
        }

        private static short SimulateGetPrfPos(short axis, out double pos, short count, out uint clk)
        {
            if (axis >= 1 && axis <= 8)
            {
                _simPos[axis] += 0.1;
                pos = _simPos[axis];
            }
            else pos = 0;
            clk = _simClock++;
            return 0;
        }

        private static short SimulateGetPrfVel(short axis, out double vel, short count, out uint clk)
        {
            if (axis < 1 || axis > 8) { vel = 0; clk = _simClock++; return -1; }
            vel = 50 + 30 * Math.Sin(_simClock * 0.01);
            clk = _simClock++;
            return 0;
        }

        private static short SimulateGetPrfAcc(short axis, out double acc, short count, out uint clk)
        {
            if (axis < 1 || axis > 8) { acc = 0; clk = _simClock++; return -1; }
            acc = 2.5 + Math.Sin(count * 0.1) * 1.5;
            clk = _simClock++;
            return 0;
        }

        private static short SimulateGetPrfMode(short axis, out int mode, short count, out uint clk)
        {
            if (axis < 1 || axis > 8) { mode = 0; clk = _simClock++; return -1; }
            mode = (count / 10) % 3;
            clk = _simClock++;
            return 0;
        }

        // ---------- 公开方法（原接口） ----------
        public short OpenDevice(short card, short mode)
        {
            if (UseSimulation) return SimulateOpen(card, mode);
            return mc.GT_Open(card, mode);
        }

        public short GT_Reset()
        {
            if (UseSimulation) return SimulateReset();
            return mc.GT_Reset();
        }

        public short CloseDevice()
        {
            if (UseSimulation) return 0;
            return mc.GT_Close();
        }

        // ---------- 新增方法（供轴控制使用） ----------
        public short GT_Stop(int mask, int option)
        {
            if (UseSimulation) return 0;
            return mc.GT_Stop(mask, option);
        }

        public short GT_AxisOn(short axis)
        {
            if (UseSimulation) return 0;
            return mc.GT_AxisOn(axis);
        }

        public short GT_AxisOff(short axis)
        {
            if (UseSimulation) return 0;
            return mc.GT_AxisOff(axis);
        }

        public short GT_ClrSts(short axis, short count)
        {
            if (UseSimulation) return 0;
            return mc.GT_ClrSts(axis, count);
        }

        /// <summary>
        /// 启动点动（Jog）运动
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="speed">速度（正负表示方向，但为保持明确，额外用 positive 参数）</param>
        /// <param name="positive">true 正向，false 负向</param>
        public short StartJog(short axis, double speed, bool positive)
        {
            if (UseSimulation)
            {
                _simVel[axis] = positive ? speed : -speed;
                return 0;
            }

            // 1. 设置为 Jog 模式
            short rt = mc.GT_PrfJog(axis);
            if (rt != 0) return rt;

            // 2. 设置 Jog 参数（可使用默认值）
            mc.TJogPrm jogPrm = new mc.TJogPrm
            {
                acc = 10,
                dec = 10,
                smooth = 0
            };
            rt = mc.GT_SetJogPrm(axis, ref jogPrm);
            if (rt != 0) return rt;

            // 3. 设置速度（正负表示方向）
            double finalSpeed = positive ? speed : -speed;
            rt = mc.GT_SetVel(axis, finalSpeed);
            if (rt != 0) return rt;

            // 4. 启动运动
            return mc.GT_Update(1 << (axis - 1));
        }

        // ---------- 原有方法（保持不变） ----------
        public short GetAxisStatus(short axis, out int status, out uint clk)
        {
            if (UseSimulation)
                return SimulateGetSts(axis, out status, 1, out clk);
            return mc.GT_GetSts(axis, out status, 1, out clk);
        }

        public short GetPrfPos(short axis, out double pos, out uint clk)
        {
            if (UseSimulation)
            {
                if (_simIsMoving[axis])
                {
                    double current = _simPos[axis];
                    double target = _simTargetPos[axis];
                    double step = 50.0; // 每次移动步长，可根据速度调整
                    if (Math.Abs(target - current) <= step)
                    {
                        _simPos[axis] = target;
                        _simIsMoving[axis] = false;
                    }
                    else
                    {
                        _simPos[axis] += Math.Sign(target - current) * step;
                    }
                }
                pos = _simPos[axis];
                clk = _simClock++;
                return 0;
            }
            //return SimulateGetPrfPos(axis, out pos, 1, out clk);
            return mc.GT_GetPrfPos(axis, out pos, 1, out clk);
        }

        public short GetPrfVel(short axis, out double vel, out uint clk)
        {
            if (UseSimulation)
                return SimulateGetPrfVel(axis, out vel, 1, out clk);
            return mc.GT_GetPrfVel(axis, out vel, 1, out clk);
        }

        public short GetPrfAcc(short axis, out double acc, out uint clk)
        {
            if (UseSimulation)
                return SimulateGetPrfAcc(axis, out acc, 1, out clk);
            return mc.GT_GetPrfAcc(axis, out acc, 1, out clk);
        }

        public short GetPrfMode(short axis, out int mode, out uint clk)
        {
            if (UseSimulation)
                return SimulateGetPrfMode(axis, out mode, 1, out clk);
            return mc.GT_GetPrfMode(axis, out mode, 1, out clk);
        }

        public short GT_EncSns(ushort sValue)
        {
            if (UseSimulation) return 0;
            return mc.GT_EncSns(sValue);
        }

        public short HomeAxis(short axis, int pos, double vel = 20, double acc = 10, int offset = 0)
        {
            if (UseSimulation) return 0;
            return mc.GT_Home(axis, pos, vel, acc, offset);
        }

        public bool CheckHomeDone(short axis)
        {
            if (UseSimulation) return true;
            ushort status = 0;
            short rt = mc.GT_HomeSts(axis, out status);
            if (rt != 0) return false;
            return status == 1;
        }

        public short MoveAbs(short axis, int targetPos, double vel = 10, double acc = 5)
        {
            if (UseSimulation)
            {
                _simTargetPos[axis] = targetPos;
                _simIsMoving[axis] = true;
                // 可选：模拟运动时间，不阻塞，由 GetPrfPos 逐步逼近
                return 0;
            }
            short rt = mc.GT_PrfTrap(axis);
            if (rt != 0) return rt;
            rt = mc.GT_SetPos(axis, targetPos);
            if (rt != 0) return rt;
            return mc.GT_Update(1 << (axis - 1));
        }

        public bool ReadDI(int ioIndex)
        {
            if (UseSimulation) return ioIndex == 0;
            int value = 0;
            short rt = mc.GT_GetDi(mc.MC_GPI, out value);
            if (rt != 0) return false;
            return (value & (1 << ioIndex)) != 0;
        }
    }
}
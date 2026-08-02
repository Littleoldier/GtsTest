using System;
using gts;

namespace GtsTest
{
    /// <summary>
    /// 模型层：封装对 gts.mc 的调用，支持模拟模式
    /// </summary>
    public class GtsModel
    {
        // 模拟模式开关（可在 Program.cs 中设置）
        public static bool UseSimulation { get; set; } = false;

        // 模拟数据存储（按轴号索引，1~8）
        private static double[] _simPos = new double[9];   // 模拟位置
        private static double[] _simVel = new double[9];   // 模拟速度
        private static double[] _simAcc = new double[9];   // 模拟加速度
        private static int[] _simStatus = new int[9];      // 模拟状态
        private static int[] _simMode = new int[9];        // 模拟运动模式
        private static bool _simInitialized = false;

        // 模拟时钟
        private static uint _simClock = 0;

        // 初始化模拟数据（可每次调用时重置）
        private static void InitializeSimulation()
        {
            if (_simInitialized) return;
            for (int i = 1; i <= 8; i++)
            {
                _simPos[i] = 0;
                _simVel[i] = 0;
                _simAcc[i] = 0;
                _simStatus[i] = 0x200;   // 默认使能 (Servo On)
                _simMode[i] = 0;          // Trap 模式
            }
            _simInitialized = true;
        }

        // ---------- 模拟方法 ----------
        private static short SimulateOpen(short card, short mode)
        {
            InitializeSimulation();
            return 0; // 模拟成功
        }

        private static short SimulateReset()
        {
            return 0; // 模拟成功
        }

        private static short SimulateGetSts(short axis, out int status, short count, out uint clk)
        {
            status = (axis >= 1 && axis <= 8) ? _simStatus[axis] : 0;
            clk = _simClock++;
            return 0;
        }

        private static short SimulateGetPrfPos(short axis, out double pos, short count, out uint clk)
        {
            // 模拟位置随时间缓慢增加（模拟运动）
            if (axis >= 1 && axis <= 8)
            {
                _simPos[axis] += 0.1; // 每次调用增加0.1，模拟运动
                pos = _simPos[axis];
            }
            else
                pos = 0;
            clk = _simClock++;
            return 0;
        }

        private static short SimulateGetPrfVel(short axis, out double vel, short count, out uint clk)
        {
            if (axis < 1 || axis > 8) 
            { 
                vel = 0; clk = _simClock++;
                return -1;
            }
            // 速度呈正弦波变化，模拟往复运动
            vel = 50 + 30 * Math.Sin(_simClock * 0.01);
            clk = _simClock++;
            return 0;
        }

        private static short SimulateGetPrfAcc(short axis, out double acc, short count, out uint clk)
        {
            if (axis < 1 || axis > 8)
            { acc = 0; clk = _simClock++;
                return -1;
            }
            // 加速度随 count 振荡，模拟实际波动
            acc = 2.5 + Math.Sin(count * 0.1) * 1.5;
            clk = _simClock++;
            return 0;
        }

        private static short SimulateGetPrfMode(short axis, out int mode, short count, out uint clk)
        {
            if (axis < 1 || axis > 8)
            {
                mode = 0; clk = _simClock++; 
                return -1;
            }
            // 模式每隔 10 次调用切换一次 (0,1,2 循环)
            mode = (count / 10) % 3;
            clk = _simClock++;
            return 0;
        }

        // ---------- 公开方法（原接口不变，内部路由到真实或模拟） ----------
        //打开设备
        public short OpenDevice(short card, short mode)
        {
            if (UseSimulation)
                return SimulateOpen(card, mode);
            else
                return mc.GT_Open(card, mode);
        }
        //设备复位
        public short GT_Reset()
        {
            if (UseSimulation)
                return SimulateReset();
            else
                return mc.GT_Reset();
        }
        //关闭设备
        public short CloseDevice()
        {
            if (UseSimulation) return 0; // 模拟模式直接成功
            return mc.GT_Close(); // 调用固高关闭函数
        }

        //读取轴状态
        public short GetAxisStatus(short axis, out int status, out uint clk)
        {
            if (UseSimulation)
                return SimulateGetSts(axis, out status, 1, out clk);
            else
                return mc.GT_GetSts(axis, out status, 1, out clk);
        }
        //读取规划位置
        public short GetPrfPos(short axis, out double pos, out uint clk)
        {
            if (UseSimulation)
                return SimulateGetPrfPos(axis, out pos, 1, out clk);
            else
                return mc.GT_GetPrfPos(axis, out pos, 1, out clk);
        }
        //读取规划速度
        public short GetPrfVel(short axis, out double vel, out uint clk)
        {
            if (UseSimulation)
                return SimulateGetPrfVel(axis, out vel, 1, out clk);
            else
                return mc.GT_GetPrfVel(axis, out vel, 1, out clk);
        }
        //读取规划加速度
        public short GetPrfAcc(short axis, out double acc, out uint clk)
        {
            if (UseSimulation)
                return SimulateGetPrfAcc(axis, out acc, 1, out clk);
            else
                return mc.GT_GetPrfAcc(axis, out acc, 1, out clk);
        }
        //读取轴运动模式
        public short GetPrfMode(short axis, out int mode, out uint clk)
        {
            if (UseSimulation)
                return SimulateGetPrfMode(axis, out mode, 1, out clk);
            else
                return mc.GT_GetPrfMode(axis, out mode, 1, out clk);
        }

        //设置编码器的计数方向
        public short GT_EncSns(ushort sValue)
        {
            if (UseSimulation)
                return 0; // 模拟成功
            else
                return mc.GT_EncSns(sValue);
        }


        /// <summary>
        /// 启动回零 (使用固高 GT_Home)
        /// </summary>
        public short HomeAxis(short axis, int pos, double vel = 20, double acc = 10, int offset = 0)
        {
            // 注意：GT_Home 的原型是 GT_Home(short axis, int pos, double vel, double acc, int offset)
            if (UseSimulation) return 0; // 模拟模式直接返回成功
            return mc.GT_Home(axis, pos, vel, acc, offset);
        }

        /// <summary>
        /// 检查回零是否完成
        /// </summary>
        public bool CheckHomeDone(short axis)
        {
            if (UseSimulation)
            {
                // 模拟模式下：假设调用 5 次后自动完成（模拟电机回零耗时）
                // 这里简单起见，直接返回 true，表示瞬间完成
                return true;
            }
            ushort status = 0;
            short rt = mc.GT_HomeSts(axis, out status);
            if (rt != 0) return false;
            return (status == 1); // 固高文档：1 表示回零完成
        }

        /// <summary>
        /// 绝对定位 (Trap 梯形曲线)
        /// </summary>
        public short MoveAbs(short axis, int targetPos, double vel = 10, double acc = 5)
        {
            if (UseSimulation)
            {
                // 模拟模式下，直接更新模拟位置（让界面上的数字跳过去）
                // 这里依靠模拟模式里的自动累加，不做特殊处理也能动
                return 0;
            }
            // 1. 设为梯形模式
            short rt = mc.GT_PrfTrap(axis);
            if (rt != 0) return rt;
            // 2. 设置目标位置
            rt = mc.GT_SetPos(axis, targetPos);
            if (rt != 0) return rt;
            // 3. 启动运动 (掩码: 1 << (axis-1))
            return mc.GT_Update(1 << (axis - 1));
        }

        /// <summary>
        /// 读取数字量输入 (DI)
        /// </summary>
        public bool ReadDI(int ioIndex)
        {
            if (UseSimulation)
            {
                // 模拟模式：比如 IO 0 永远返回 true，其他返回 false，便于演示流程
                return ioIndex == 0;
            }
            int value = 0;
            short rt = mc.GT_GetDi(mc.MC_GPI, out value);
            if (rt != 0) return false;
            // 按位取出指定的 IO (ioIndex 范围 0~15)
            return (value & (1 << ioIndex)) != 0;
        }
    }
}
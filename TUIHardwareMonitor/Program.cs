// See https://aka.ms/new-console-template for more information
using LibreHardwareMonitor.Hardware;
using System.Threading;
using System.IO.Ports;
SerialPort port = new SerialPort();
Computer computer = new Computer
{
    IsCpuEnabled = true,
    IsGpuEnabled = true,
    IsMemoryEnabled = true
};
computer.Open();
port.Parity = Parity.None;
port.StopBits = StopBits.One;
port.DataBits = 8;
port.Handshake = Handshake.None;
port.RtsEnable = true;
string[] ports = SerialPort.GetPortNames();
int portNum = 0;
foreach (string portn in ports)
{
    
    Console.WriteLine(portNum + " . " + portn);

}
if (ports.Length == 1)
{
    port.PortName = ports[0];
    port.BaudRate = 9600;
    port.Open();
}
if (ports.Length > 1)
{
    Console.Write("Enter the port num : ");
    int res = int.Parse(Console.ReadLine());
    port.PortName = ports[res];
    port.BaudRate = 9600;
    port.Open();
}
while (true)
{
    float? memusd = 0;
    float? cpu = 0;
    float? gpu = 0;
    float? memtot = 0;
    string? cpuN ="cpu";
    string? gpuN = "gpu";
    computer.Accept(new UpdateVisitor());
    foreach (IHardware hardware in computer.Hardware)
    {
        if (hardware.HardwareType == HardwareType.Cpu)
        {
            Console.WriteLine(hardware.Name);
            cpuN = hardware.Name;
        }
        if (hardware.HardwareType == HardwareType.GpuNvidia)
        {
            Console.WriteLine(hardware.Name);
            gpuN = hardware.Name;
        }
        if (hardware.HardwareType == HardwareType.GpuAmd)
        {
            Console.WriteLine(hardware.Name);
            gpuN = hardware.Name;
        }
        if (hardware.HardwareType == HardwareType.GpuIntel)
        {
            Console.WriteLine(hardware.Name);
            gpuN = hardware.Name;
        }
        if (hardware.HardwareType == HardwareType.Memory)
        {
            Console.WriteLine("Memory : ");
        }
        foreach (ISensor sensor in hardware.Sensors)
        {

            if (sensor.Name == "CPU Total")
            {
                cpu = sensor.Value;
                Console.WriteLine("Cpu Usage : " + sensor.Value);
            }
            if (sensor.Name == "D3D 3D")
            {
                gpu = sensor.Value;
                Console.WriteLine("Gpu Usage : " + sensor.Value);
            }
            if (sensor.Name == "Memory Used")
            {
                memusd = sensor.Value;
            }
            if (sensor.Name == "Memory Available")
            {
                memtot = memusd + sensor.Value;
                Console.WriteLine("Memory Used : " + memusd + "GB / " + memtot + "GB");
            }
        }
    }
    float cpuRo = cpu ?? 0;
    float gpuRo = gpu ?? 0;
    float memusdRo = memusd ?? 0;
    float memtotRo = memtot ?? 0;
    try
    {
        string[] cpuS = cpuN.Split(' ');
        string cpuR = "";
        for (int i=1; i<cpuS.Length; i++)
        {
            cpuR = cpuR + cpuS[i];
        }
        string[] gpuS = gpuN.Split(" ");
        string gpuR = "";
        for (int i = 1; i < gpuS.Length; i++)
        {
            gpuR = gpuR + gpuS[i];
        }
        port.Write(Math.Round(cpuRo, 2) + "*" + Math.Round(gpuRo, 2) + "#" + Math.Round(memusdRo, 2) + "!" + Math.Round(memtotRo, 2) + "_" + cpuR + "{" + gpuR + "}");
    }
    catch(Exception ex)
    {
        Console.WriteLine("Arduino isn't responding");
        Console.WriteLine(ex.ToString());
    }
    Thread.Sleep(1000);
    Console.Clear();
}
public class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }
    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
    }
    public void VisitSensor(ISensor sensor) { }
    public void VisitParameter(IParameter parameter) { }
}

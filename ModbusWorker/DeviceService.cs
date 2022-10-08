using ModbusWorker.Commands;
using ModbusWorker.ModbusModels;

namespace ModbusWorker
{
    public static class DeviceService
    {
        public static SessionInfo? GetSessionInfo(string deviceSecret)
        {
            return new SessionInfo
            {
                DeviceId = "001",
                ProtocolType = ModbusProtocolType.ModbusRtu,
                DeviceName = deviceSecret,
            };
        }

        public static List<ModbusPropertyInfo> GetExtensionProperties(string deviceId)
        {
            int osh = 200, osl = -50, ish = 4095, isl = 819;
            string expression = $"({osh}-{osl})*(x-{isl})/({ish}-{isl})+{osl}";

            var model = new List<ModbusPropertyInfo>
            {
                new ModbusPropertyInfo
                {
                    CorrelationId = "WaterTemperature",
                    UnitId = 0x01,
                    StartAddress = "0x0005",
                    DataType = new ModbusOriginalDataType
                    {
                        Type = ModbusDataType.UInt16,
                        Specs = new OriginalDataTypeSpecs
                        {
                            SwapByte=false,
                            ReverseRegister=false
                        }
                    },
                    OperateType = ModbusOperateType.InputRegister,
                    PollingInterval = 5000,
                    ReportMethod = ModbusReportMethod.Timing,
                    Scaling = 1,
                    Expression=expression

                },
                new ModbusPropertyInfo
                {
                    CorrelationId = "Temperature",
                    UnitId = 0x03,
                    StartAddress = "0x0200",
                    DataType = new ModbusOriginalDataType
                    {
                        Type = ModbusDataType.UInt16,
                        Specs = new OriginalDataTypeSpecs
                        {
                            SwapByte=false,
                            ReverseRegister=false
                        }
                    },
                    OperateType = ModbusOperateType.InputRegister,
                    PollingInterval = 5000,
                    ReportMethod = ModbusReportMethod.Timing,
                    Scaling = 0.1
                },
                new ModbusPropertyInfo
                {
                    CorrelationId = "Humidity",
                    UnitId = 0x03,
                    StartAddress = "0x0201",
                    DataType = new ModbusOriginalDataType
                    {
                        Type = ModbusDataType.UInt16,
                        Specs = new OriginalDataTypeSpecs
                        {
                            SwapByte=false,
                            ReverseRegister=false
                        }
                    },
                    OperateType = ModbusOperateType.InputRegister,
                    PollingInterval = 5000,
                    ReportMethod = ModbusReportMethod.Timing,
                    Scaling = 0.1
                },
                //new ModbusPropertyInfo
                //{
                //    CorrelationId = "Humidity",
                //    UnitId = 0x03,
                //    StartAddress = "0x3344",
                //    DataType = new ModbusOriginalDataType
                //    {
                //        Type = ModbusDataType.UInt16,
                //        Specs = new OriginalDataTypeSpecs
                //        {
                //            SwapByte=false,
                //            ReverseRegister=false
                //        }
                //    },
                //    OperateType = ModbusOperateType.HoldingRegister,
                //    PollingInterval = 6000,
                //    ReportMethod = ModbusReportMethod.Timing,
                //    Scaling = 0.01,
                //    WriteFunctionCode = (byte)ModbusFunctionCode.WriteSingleRegister
                //},
                //new ModbusPropertyInfo
                //{
                //    CorrelationId = "SerialNumber",
                //    UnitId = 0x04,
                //    StartAddress = "0x5566",
                //    DataType = new ModbusOriginalDataType
                //    {
                //        Type = ModbusDataType.String,
                //        Specs = new OriginalDataTypeSpecs
                //        {
                //            RegisterCount=10
                //        }
                //    },
                //    OperateType = ModbusOperateType.InputRegister,
                //    PollingInterval = 6000,
                //    ReportMethod = ModbusReportMethod.Timing,
                //},
                //new ModbusPropertyInfo
                //{
                //    CorrelationId = "SwitchStatus",
                //    UnitId = 0x05,
                //    StartAddress = "0x7788",
                //    DataType = new ModbusOriginalDataType
                //    {
                //        Type = ModbusDataType.Bool
                //    },
                //    OperateType = ModbusOperateType.CoilStatus,
                //    PollingInterval = 6000,
                //    ReportMethod = ModbusReportMethod.Timing,
                //    WriteFunctionCode= (byte)ModbusFunctionCode.WriteMultipleCoils
                //}
            };

            return model;
        }
    }
}
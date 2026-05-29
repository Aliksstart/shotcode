using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace Tests
{
    [TestClass]
    public class BlockLayoutTest
    {
        private int _version_size = 1;
        private int _type_size = 1;
        private int _digits_size = 1;
        private int _algorithm_size = 1;
        private int _ts_created_size = 8;
        private int _ts_updated_size = 8;
        private int _period_or_counter_size = 8;
        private int _len_service_name_size = 4;
        private int _len_sercret_size = 4;
        private int _len_extra_size = 4;
        [TestMethod]
        public void BlockLayout_ShouldMatchActualFields()
        {
            Assert.AreEqual(_version_size, BlockLayout.VersionSize);
            Assert.AreEqual(_type_size, BlockLayout.TypeSize);
            Assert.AreEqual(_digits_size, BlockLayout.DigitsSize);
            Assert.AreEqual(_algorithm_size, BlockLayout.AlgorithmSize);
            Assert.AreEqual(_ts_created_size, BlockLayout.TimestampSize);
            Assert.AreEqual(_ts_updated_size, BlockLayout.TimestampSize);
            Assert.AreEqual(_period_or_counter_size, BlockLayout.PeriodOrCounterSize);
            Assert.AreEqual(_len_service_name_size, BlockLayout.LengthSize);
            Assert.AreEqual(_len_sercret_size, BlockLayout.LengthSize);
            Assert.AreEqual(_len_extra_size, BlockLayout.LengthSize);
            int min_block_size = _version_size + _type_size + _digits_size + _algorithm_size + _ts_created_size +
                _ts_updated_size + _period_or_counter_size + _len_service_name_size + _len_sercret_size + _len_extra_size;
            Assert.AreEqual(min_block_size, BlockLayout.MinBlockSize);
        }
        [TestMethod]
        public void BlockLayout_ShouldMatchOffsets()
        {
            int version_offset = 0;
            int type_offset = version_offset + _version_size;
            int digits_offset = type_offset + _type_size;
            int algorithm_offset = digits_offset + _digits_size;
            int ts_created_offset = algorithm_offset + _algorithm_size;
            int ts_updated_offset = ts_created_offset + _ts_created_size;
            int period_or_counter_offset = ts_updated_offset + _ts_updated_size;
            int len_service_name_offset = period_or_counter_offset + _period_or_counter_size;
            int len_sercret_offset = len_service_name_offset + _len_service_name_size;
            int len_extra_offset = len_sercret_offset + _len_sercret_size;
            Assert.AreEqual(version_offset, BlockLayout.VersionOffset);
            Assert.AreEqual(type_offset, BlockLayout.TypeOffset);
            Assert.AreEqual(digits_offset, BlockLayout.DigitsOffset);
            Assert.AreEqual(algorithm_offset, BlockLayout.AlgorithmOffset);
            Assert.AreEqual(ts_created_offset, BlockLayout.CreatedTSOffset);
            Assert.AreEqual(ts_updated_offset, BlockLayout.UpdatedTSOffset);
            Assert.AreEqual(period_or_counter_offset, BlockLayout.PeriodOrCounterOffset);
            Assert.AreEqual(len_service_name_offset, BlockLayout.ServiceNameLengthOffset);
            Assert.AreEqual(len_sercret_offset, BlockLayout.SecretLengthOffset);
            Assert.AreEqual(len_extra_offset, BlockLayout.ExtraLengthOffset);

            int data_offset = len_extra_offset + _len_extra_size;
            Assert.AreEqual(data_offset, BlockLayout.DataOffset);
            Assert.AreEqual(data_offset, BlockLayout.ServiceNameOffset);
            Assert.AreEqual(data_offset, BlockLayout.MinBlockSize);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfCalculator
{
    internal class Memory
    {
        private List<double> _memoryStack = new List<double>();

        public event Action MemoryUpdated; 

        public void Store(double value)
        {
            _memoryStack.Add(value);
            MemoryUpdated?.Invoke();
        }

        public double Recall()
        {
            return _memoryStack.Count > 0 ? _memoryStack.Last() : 0;
        }

        public void Add(double value)
        {
            if (_memoryStack.Count > 0)
            {
                _memoryStack[_memoryStack.Count - 1] += value;
            }
            else
            {
                _memoryStack.Add(value);
            }
            MemoryUpdated?.Invoke(); 
        }

        public void Subtract(double value)
        {
            if (_memoryStack.Count > 0)
            {
                _memoryStack[_memoryStack.Count - 1] -= value;
            }
            else
            {
                _memoryStack.Add(-value);
            }
            MemoryUpdated?.Invoke(); 
        }

        public void Clear()
        {
            _memoryStack.Clear();
            MemoryUpdated?.Invoke(); 
        }

        public List<double> GetMemoryList()
        {
            return new List<double>(_memoryStack);
        }
    }
}

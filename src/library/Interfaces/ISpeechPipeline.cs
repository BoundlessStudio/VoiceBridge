using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceBridge.Interfaces;

public interface ISpeechPipeline
{
  void Clear();
  void Start();
  void Stop();
}
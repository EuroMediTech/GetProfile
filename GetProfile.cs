using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS
{
  public class Script
  {
    public Script() { }

    private DoseProfile CalculateProfile(
      Double x_start, Double y_start, Double z_start, Double x_stop, Double y_stop, Double z_stop, Double step, Dose dose
    )
    {
      // 始点と終点を定義
      var start = new VVector(x_start, y_start, z_start);
      var stop = new VVector(x_stop, y_stop, z_stop);

      // 始点・終点間距離を定義
      var distance = VVector.Distance(start, stop);

      if (distance == 0)
      {
        throw new ApplicationException("Distance between two points is 0.");
      }

      // 距離をステップサイズで割ってサンプリング点数を決定（切り捨て）
      Double point = Math.Floor(distance / step);

      if (point < 1.0)
      {
        throw new ApplicationException("Step size is largar than distance between two points.");
      }

      // 始点から終点へ向かう長さ1のベクトルを得る
      var dir = stop - start;
      dir.ScaleToUnitLength();

      // stepサイズに合うように終点位置を調整
      var stop_new = start + dir * point * step;

      // DoseProfile用の配列を準備
      double[] profile = new double[(int) (point + 1)];
      // DoseProfileを取得
      var doseProfile = dose.GetDoseProfile(start, stop_new, profile);

      return doseProfile;
    }

    public void Execute(ScriptContext context /*, System.Windows.Window window*/ )
    {
      String inputDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
      String inputFile = inputDir + "\\input.csv";

      var patient = context.Patient;
      var planSetup = context.PlanSetup;

      if (planSetup == null)
      {
        throw new ApplicationException("Please Open the plan.");
      }

      var planDose = planSetup.Dose;
      if (planDose == null)
      {
        throw new ApplicationException("Please Calculate the plan.");
      }

      bool CanGetBeamDose = true;

      // BeamDoseを取得できるかどうか確認する。
      Type t = typeof(Beam);
      System.Reflection.PropertyInfo info = t.GetProperty("Dose");
      if (info == null)
      {
        CanGetBeamDose = false;
      }

      Double x_start;
      Double y_start;
      Double z_start;

      Double x_stop;
      Double y_stop;
      Double z_stop;

      Double step;

      int i = 1;

      string msg = null;

      try
      {
        using(var sr = new StreamReader(inputFile))
        {
          while (!sr.EndOfStream)
          {
            var profileList = new List<DoseProfile>();

            var line = sr.ReadLine();
            var values = line.Split(',');

            if (values[0].StartsWith("x"))
            {
              continue;
            }

            x_start = double.Parse(values[0]);
            y_start = double.Parse(values[1]);
            z_start = double.Parse(values[2]);
            x_stop = double.Parse(values[3]);
            y_stop = double.Parse(values[4]);
            z_stop = double.Parse(values[5]);
            step = double.Parse(values[6]);

            var outputFile = inputDir + String.Format("\\Profile{0:00}.csv", i);
            string header = "x, y, z, plan";

            profileList.Add(CalculateProfile(x_start, y_start, z_start, x_stop, y_stop, z_stop, step, planDose));
            var count = profileList[0].Count;

            if (CanGetBeamDose)
            {
              foreach (var beam in planSetup.Beams)
              {
                if (!beam.IsSetupField)
                {
                  header += String.Format(", {0}", beam.Id);
                  profileList.Add(CalculateProfile(x_start, y_start, z_start, x_stop, y_stop, z_stop, step, beam.Dose));
                }
              }
            }

            using(var sw = new StreamWriter(outputFile))
            {
              sw.WriteLine(header);

              for (int j = 0; j < count; j++)
              {
                string data = String.Format("{0}, {1}, {2}", profileList[0][j].Position.x, profileList[0][j].Position.y, profileList[0][j].Position.z);
                foreach (var profile in profileList)
                {
                  data += String.Format(", {0}", profile[j].Value);
                }

                sw.WriteLine(data);

              }
            }

            msg += String.Format("Profile{0:00} is written to {1}.\n", i, outputFile);

            i++;
          }
        }
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message);
      }

      MessageBox.Show(msg);

    }
  }
}
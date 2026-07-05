using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace DataConcentrator
{
    public class AlarmRaisedEventArgs : EventArgs
    {
        public int AlarmId { get; set; }
    }

    public class TagManager
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        private static TagManager _instance;
        public  static TagManager Instance =>
            _instance ?? (_instance = new TagManager());

        // ── Observable collections (WPF binds to these) ───────────────────────
        public ObservableCollection<Tag>   Tags   { get; } = new ObservableCollection<Tag>();
        public ObservableCollection<Alarm> Alarms { get; } = new ObservableCollection<Alarm>();

        // ── Alarm raised event ────────────────────────────────────────────────
        public event EventHandler<AlarmRaisedEventArgs> AlarmRaised;

        // ── Lock for PLC read/write ───────────────────────────────────────────
        private static readonly object _plcLock = new object();

        // ── Constructor: load persisted data from DB ──────────────────────────
        private TagManager()
        {
            try
            {
                var ctx = ContextClass.Instance;

                // Load all four tag types into the shared observable collection
                foreach (var t in ctx.AnalogInputs.ToList())   Tags.Add(t);
                foreach (var t in ctx.AnalogOutputs.ToList())  Tags.Add(t);
                foreach (var t in ctx.DigitalInputs.ToList())  Tags.Add(t);
                foreach (var t in ctx.DigitalOutputs.ToList()) Tags.Add(t);

                foreach (var a in ctx.Alarms.ToList()) Alarms.Add(a);

                // Restart scan threads for input tags that were scanning
                foreach (var tag in Tags.OfType<InputTag>().Where(t => t.ScanEnabled))
                    StartScanThread(tag);

                Logger.Log(TraceCategory.TagAdd, "TagManager initialised — loaded tags from DB");
            }
            catch (Exception ex)
            {
                Logger.Log(TraceCategory.Error, $"TagManager init failed: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  TAG CRUD
        // ═══════════════════════════════════════════════════════════════════════

        public void AddTag(Tag tag)
        {
            try
            {
                var ctx = ContextClass.Instance;

                // Add to the correct typed DbSet
                if      (tag is AnalogInput  ai) ctx.AnalogInputs.Add(ai);
                else if (tag is AnalogOutput ao) ctx.AnalogOutputs.Add(ao);
                else if (tag is DigitalInput di) ctx.DigitalInputs.Add(di);
                else if (tag is DigitalOutput d) ctx.DigitalOutputs.Add(d);

                ctx.SaveChanges();
                Tags.Add(tag);

                if (tag is InputTag input && input.ScanEnabled)
                    StartScanThread(input);

                Logger.Log(TraceCategory.TagAdd, $"TAG_ADD name={tag.Name} type={tag.TagType}");
            }
            catch (Exception ex)
            {
                Logger.Log(TraceCategory.Error, $"AddTag failed: {ex.Message}");
                throw;
            }
        }

        public void RemoveTag(string tagName)
        {
            try
            {
                StopScanThread(tagName);

                var ctx = ContextClass.Instance;

                // Remove from whichever typed DbSet holds this tag
                var ai = ctx.AnalogInputs.Find(tagName);
                if (ai != null) ctx.AnalogInputs.Remove(ai);

                var ao = ctx.AnalogOutputs.Find(tagName);
                if (ao != null) ctx.AnalogOutputs.Remove(ao);

                var di = ctx.DigitalInputs.Find(tagName);
                if (di != null) ctx.DigitalInputs.Remove(di);

                var doo = ctx.DigitalOutputs.Find(tagName);
                if (doo != null) ctx.DigitalOutputs.Remove(doo);

                // Remove any alarms that reference this tag
                var related = ctx.Alarms.Where(a => a.TagName == tagName).ToList();
                ctx.Alarms.RemoveRange(related);
                foreach (var a in related)
                {
                    var mem = Alarms.FirstOrDefault(x => x.Id == a.Id);
                    if (mem != null) Alarms.Remove(mem);
                }

                ctx.SaveChanges();

                var memTag = Tags.FirstOrDefault(t => t.Name == tagName);
                if (memTag != null) Tags.Remove(memTag);

                Logger.Log(TraceCategory.TagAdd, $"TAG_REMOVE name={tagName}");
            }
            catch (Exception ex)
            {
                Logger.Log(TraceCategory.Error, $"RemoveTag failed: {ex.Message}");
                throw;
            }
        }

        public void UpdateTag(Tag updated)
        {
            try
            {
                var ctx = ContextClass.Instance;

                if (updated is AnalogInput ai)
                {
                    var e = ctx.AnalogInputs.Find(updated.Name);
                    if (e != null) ctx.Entry(e).CurrentValues.SetValues(ai);
                }
                else if (updated is AnalogOutput ao)
                {
                    var e = ctx.AnalogOutputs.Find(updated.Name);
                    if (e != null) ctx.Entry(e).CurrentValues.SetValues(ao);
                }
                else if (updated is DigitalInput di)
                {
                    var e = ctx.DigitalInputs.Find(updated.Name);
                    if (e != null) ctx.Entry(e).CurrentValues.SetValues(di);
                }
                else if (updated is DigitalOutput doo)
                {
                    var e = ctx.DigitalOutputs.Find(updated.Name);
                    if (e != null) ctx.Entry(e).CurrentValues.SetValues(doo);
                }

                ctx.SaveChanges();
                Logger.Log(TraceCategory.Update, $"TAG_UPDATE name={updated.Name}");
            }
            catch (Exception ex)
            {
                Logger.Log(TraceCategory.Error, $"UpdateTag failed: {ex.Message}");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  ALARM CRUD
        // ═══════════════════════════════════════════════════════════════════════

        public void AddAlarm(Alarm alarm)
        {
            try
            {
                var ctx = ContextClass.Instance;
                ctx.Alarms.Add(alarm);
                ctx.SaveChanges();
                Alarms.Add(alarm);
                Logger.Log(TraceCategory.TagAdd, $"ALARM_ADD id={alarm.Id} tag={alarm.TagName}");
            }
            catch (Exception ex)
            {
                Logger.Log(TraceCategory.Error, $"AddAlarm failed: {ex.Message}");
                throw;
            }
        }

        public void RemoveAlarm(int alarmId)
        {
            try
            {
                var ctx = ContextClass.Instance;
                var alarm = ctx.Alarms.Find(alarmId);
                if (alarm != null) { ctx.Alarms.Remove(alarm); ctx.SaveChanges(); }

                var mem = Alarms.FirstOrDefault(a => a.Id == alarmId);
                if (mem != null) Alarms.Remove(mem);

                Logger.Log(TraceCategory.TagAdd, $"ALARM_REMOVE id={alarmId}");
            }
            catch (Exception ex)
            {
                Logger.Log(TraceCategory.Error, $"RemoveAlarm failed: {ex.Message}");
                throw;
            }
        }

        public void AcknowledgeAlarm(int alarmId)
        {
            var alarm = Alarms.FirstOrDefault(a => a.Id == alarmId);
            if (alarm == null) return;
            alarm.State = AlarmState.Acknowledged;
            Logger.Log(TraceCategory.AlarmAck, $"ACK alarmId={alarmId} tag={alarm.TagName}");
        }

        public void UpdateAlarm(Alarm updated)
        {
            try
            {
                var ctx = ContextClass.Instance;
                var existing = ctx.Alarms.Find(updated.Id);
                if (existing == null) return;

                // Update only the editable fields — Id and TagName stay fixed
                existing.Limit       = updated.Limit;
                existing.IsHighAlarm = updated.IsHighAlarm;
                existing.Message     = updated.Message;
                ctx.SaveChanges();

                // Sync the in-memory collection so the UI reflects the change
                var mem = Alarms.FirstOrDefault(a => a.Id == updated.Id);
                if (mem != null)
                {
                    mem.Limit       = updated.Limit;
                    mem.IsHighAlarm = updated.IsHighAlarm;
                    mem.Message     = updated.Message;
                }

                Logger.Log(TraceCategory.Update, $"ALARM_UPDATE id={updated.Id} tag={updated.TagName}");
            }
            catch (Exception ex)
            {
                Logger.Log(TraceCategory.Error, $"UpdateAlarm failed: {ex.Message}");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  WRITE TO OUTPUT TAGS
        // ═══════════════════════════════════════════════════════════════════════

        public void WriteAnalogOutput(string tagName, double value)
        {
            try
            {
                var tag = Tags.OfType<AnalogOutput>().FirstOrDefault(t => t.Name == tagName);
                if (tag == null) return;
                lock (_plcLock) { PLC.Instance.SetAnalogValue(tag.IOAddress, value); }
                tag.CurrentValue = value;
                Logger.Log(TraceCategory.TagWrite, $"TAG_WRITE name={tagName} value={value}");
            }
            catch (Exception ex)
            {
                Logger.Log(TraceCategory.Error, $"WriteAnalogOutput failed: {ex.Message}");
                throw;
            }
        }

        public void WriteDigitalOutput(string tagName, bool value)
        {
            try
            {
                var tag = Tags.OfType<DigitalOutput>().FirstOrDefault(t => t.Name == tagName);
                if (tag == null) return;
                lock (_plcLock) { PLC.Instance.SetDigitalValue(tag.IOAddress, value ? 1.0 : 0.0); }
                tag.CurrentValue = value;
                Logger.Log(TraceCategory.TagWrite, $"TAG_WRITE name={tagName} value={value}");
            }
            catch (Exception ex)
            {
                Logger.Log(TraceCategory.Error, $"WriteDigitalOutput failed: {ex.Message}");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  SCAN ON/OFF
        // ═══════════════════════════════════════════════════════════════════════

        public void SetScanEnabled(string tagName, bool enabled)
        {
            var tag = Tags.OfType<InputTag>().FirstOrDefault(t => t.Name == tagName);
            if (tag == null) return;
            tag.ScanEnabled = enabled;
            if (enabled) StartScanThread(tag); else StopScanThread(tagName);
            UpdateTag(tag);
            Logger.Log(TraceCategory.Update, $"SCAN_{(enabled ? "ON" : "OFF")} name={tagName}");
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  REPORT
        // ═══════════════════════════════════════════════════════════════════════

        public string GenerateReport()
        {
            var lines = new System.Text.StringBuilder();
            lines.AppendLine($"SCADA Report — generated {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            lines.AppendLine(new string('─', 60));

            foreach (var ai in Tags.OfType<AnalogInput>())
            {
                double mid  = (ai.HighLimit + ai.LowLimit) / 2.0;
                double low  = mid - 5.0;
                double high = mid + 5.0;

                // Fetch all recorded values for this tag that fall in [mid-5, mid+5]
                var records = ContextClass.Instance.TagValueRecords
                    .Where(r => r.TagName == ai.Name && r.Value >= low && r.Value <= high)
                    .OrderBy(r => r.Timestamp)
                    .ToList();

                if (records.Count == 0) continue;

                lines.AppendLine($"  Tag: {ai.Name} (mid={mid:G}, range [{low:G}, {high:G}])");

                foreach (var rec in records)
                    lines.AppendLine(
                        $"    Tag: {rec.TagName,-20} | Value: {rec.Value,-12:G6} | Time: {rec.Timestamp:yyyy-MM-dd HH:mm:ss}");

                lines.AppendLine();
            }

            Logger.Log(TraceCategory.ImportExport, "REPORT_GENERATED");
            return lines.ToString();
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  SCAN THREAD MANAGEMENT
        // ═══════════════════════════════════════════════════════════════════════

        private void StartScanThread(InputTag tag)
        {
            if (PLC.tagThreads.ContainsKey(tag.Name)) return;

            var thread = new Thread(() => ScanLoop(tag))
            {
                IsBackground = true,
                Name = $"Scan_{tag.Name}"
            };

            PLC.tagThreads[tag.Name] = thread;
            thread.Start();
        }

        private void StopScanThread(string tagName)
        {
            if (PLC.tagThreads.TryGetValue(tagName, out Thread t))
            {
                try { t.Abort(); } catch { }
                PLC.tagThreads.Remove(tagName);
            }
        }

        private void ScanLoop(InputTag tag)
        {
            double lastAnalogValue = double.MinValue;

            while (true)
            {
                try
                {
                    if (tag.ScanEnabled)
                    {
                        if (tag is AnalogInput ai)
                        {
                            double raw;
                            lock (_plcLock) { raw = PLC.Instance.GetAnalogValue(ai.IOAddress); }

                            if (Math.Abs(raw - lastAnalogValue) >= Math.Abs(ai.Deadband))
                            {
                                ai.CurrentValue = raw;
                                lastAnalogValue = raw;

                                // Persist every accepted value for report generation
                                try
                                {
                                    ContextClass.Instance.TagValueRecords.Add(new TagValueRecord
                                    {
                                        TagName   = ai.Name,
                                        Value     = raw,
                                        Timestamp = DateTime.Now
                                    });
                                    ContextClass.Instance.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    Logger.Log(TraceCategory.Error, $"TagValueRecord save failed: {ex.Message}");
                                }

                                CheckAlarms(ai);
                            }
                        }
                        else if (tag is DigitalInput di)
                        {
                            bool raw;
                            lock (_plcLock) { raw = PLC.Instance.GetAnalogValue(di.IOAddress) >= 0.5; }
                            di.CurrentValue = raw;
                        }
                    }
                }
                catch (ThreadAbortException) { return; }
                catch (Exception ex)
                {
                    Logger.Log(TraceCategory.Error, $"ScanLoop error tag={tag.Name}: {ex.Message}");
                }

                Thread.Sleep(tag.ScanTime);
            }
        }

        private void CheckAlarms(AnalogInput ai)
        {
            var tagAlarms = Alarms.Where(a => a.TagName == ai.Name).ToList();

            foreach (var alarm in tagAlarms)
            {
                bool shouldFire = alarm.IsHighAlarm
                    ? ai.CurrentValue > alarm.Limit + Math.Abs(ai.Hysteresis)
                    : ai.CurrentValue < alarm.Limit - Math.Abs(ai.Hysteresis);

                if (shouldFire && alarm.State == AlarmState.Inactive)
                {
                    alarm.State = AlarmState.Active;

                    var record = new ActivatedAlarm
                    {
                        AlarmId   = alarm.Id,
                        TagName   = ai.Name,
                        Message   = alarm.Message,
                        Timestamp = DateTime.Now
                    };

                    try
                    {
                        ContextClass.Instance.ActivatedAlarms.Add(record);
                        ContextClass.Instance.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(TraceCategory.Error, $"Failed to persist alarm: {ex.Message}");
                    }

                    Logger.Log(TraceCategory.AlarmRaised,
                        $"ALARM_RAISED id={alarm.Id} tag={ai.Name} val={ai.CurrentValue}");

                    AlarmRaised?.Invoke(this, new AlarmRaisedEventArgs { AlarmId = alarm.Id });
                }
                else if (!shouldFire && alarm.State != AlarmState.Inactive)
                {
                    alarm.State = AlarmState.Inactive;
                }
            }
        }
    }
}

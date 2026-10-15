using System;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace LittleDart.Common.Duty;

public static class DutyClassifier
{
	private const int FailedProbeIntervalMs = 500;

	private static uint _cachedTerritoryId;

	private static bool _hasResult;

	private static int _cachedPerParty;

	private static int _cachedParties;

	private static uint _lastProbeTerritoryId;

	private static long _lastProbeTicks;

	public static bool IsInDuty => Svc.Condition[(ConditionFlag)34];

	public static int MembersPerParty
	{
		get
		{
			EnsureCache();
			return _cachedPerParty;
		}
	}

	public static int PartyCount
	{
		get
		{
			EnsureCache();
			return _cachedParties;
		}
	}

	public static int TotalMembers
	{
		get
		{
			EnsureCache();
			if (_cachedParties <= 0)
			{
				return _cachedPerParty;
			}
			return _cachedPerParty * _cachedParties;
		}
	}

	public static bool IsFourManDuty
	{
		get
		{
			if (IsInDuty)
			{
				return MembersPerParty == 4;
			}
			return false;
		}
	}

	public static bool IsEightManDuty
	{
		get
		{
			if (IsInDuty)
			{
				return TotalMembers == 8;
			}
			return false;
		}
	}

	public static string Describe()
	{
		if (!IsInDuty)
		{
			return "副本外";
		}
		int membersPerParty = MembersPerParty;
		if (membersPerParty <= 0)
		{
			return "副本内（人数判不出来）";
		}
		int partyCount = PartyCount;
		if (partyCount > 0 && partyCount != 1)
		{
			return $"副本内 · {membersPerParty} 人队 × {partyCount} = {TotalMembers} 人";
		}
		return membersPerParty switch
		{
			4 => "四人副本",
			8 => "八人副本",
			_ => $"副本内 · 每队 {membersPerParty} 人",
		};
	}

	public static void Invalidate()
	{
		_cachedTerritoryId = 0u;
		_hasResult = false;
		_cachedPerParty = 0;
		_cachedParties = 0;
		_lastProbeTerritoryId = 0u;
		_lastProbeTicks = 0L;
	}

	private static void EnsureCache()
	{
		uint territoryType = Svc.ClientState.TerritoryType;
		if ((_hasResult && territoryType == _cachedTerritoryId) || territoryType == 0)
		{
			return;
		}
		if (territoryType != _cachedTerritoryId)
		{
			_hasResult = false;
			_cachedTerritoryId = 0u;
			_cachedPerParty = 0;
			_cachedParties = 0;
		}
		long tickCount = Environment.TickCount64;
		if (_lastProbeTerritoryId != territoryType || tickCount - _lastProbeTicks >= 500)
		{
			_lastProbeTerritoryId = territoryType;
			_lastProbeTicks = tickCount;
			if (TryReadMembers(territoryType, out var perParty, out var parties))
			{
				_hasResult = true;
				_cachedTerritoryId = territoryType;
				_cachedPerParty = perParty;
				_cachedParties = parties;
			}
		}
	}

	private static bool TryReadMembers(uint territoryId, out int perParty, out int parties)
	{
		perParty = 0;
		parties = 0;
		try
		{
			ExcelSheet<TerritoryType> excelSheet = Svc.Data.GetExcelSheet<TerritoryType>((ClientLanguage?)null, (string)null);
			if (excelSheet == null)
			{
				return false;
			}
			TerritoryType? rowOrDefault = excelSheet.GetRowOrDefault(territoryId);
			if (!rowOrDefault.HasValue)
			{
				return false;
			}
			TerritoryType territoryRow = rowOrDefault.Value;
			RowRef<ContentFinderCondition> contentFinderCondition = territoryRow.ContentFinderCondition;
			if (contentFinderCondition.RowId == 0)
			{
				return false;
			}
			ContentFinderCondition contentFinderRow = contentFinderCondition.Value;
			RowRef<ContentMemberType> contentMemberType = contentFinderRow.ContentMemberType;
			if (contentMemberType.RowId == 0)
			{
				return false;
			}
			ContentMemberType memberTypeRow = contentMemberType.Value;
			int perPartyCount = memberTypeRow.TanksPerParty + memberTypeRow.HealersPerParty + memberTypeRow.MeleesPerParty + memberTypeRow.RangedPerParty;
			if (perPartyCount <= 0)
			{
				return false;
			}
			perParty = perPartyCount;
			parties = memberTypeRow.PartyCount;
			return true;
		}
		catch
		{
			return false;
		}
	}
}

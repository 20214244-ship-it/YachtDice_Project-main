using System.Linq;
using System.Collections.Generic;

public enum Category
{
    Ones,           // 에이스
    Twos,           // 듀얼
    Threes,         // 트리플
    Fours,          // 쿼드
    Fives,          // 펜타
    Sixes,          // 헥사
    ThreeOfAKind,   // 쓰리 오브 어 카인드
    FourOfAKind,    // 포 오브 어 카인드
    FullHouse,      // 풀하우스
    SmallStraight,  // 스몰 스트레이트
    LargeStraight,  // 라지 스트레이트
    Yacht,          // 요트
    Choice          // 찬스
}

public static class ScoreCalculator
{
    public static int Calculate(Category cat, int[] dice)
    {
        switch (cat)
        {
            case Category.Ones:   return dice.Where(d => d == 1).Sum();
            case Category.Twos:   return dice.Where(d => d == 2).Sum();
            case Category.Threes: return dice.Where(d => d == 3).Sum();
            case Category.Fours:  return dice.Where(d => d == 4).Sum();
            case Category.Fives:  return dice.Where(d => d == 5).Sum();
            case Category.Sixes:  return dice.Where(d => d == 6).Sum();

            case Category.Choice: return dice.Sum();

            case Category.ThreeOfAKind:
                return dice.GroupBy(d => d).Any(g => g.Count() >= 3) ? dice.Sum() : 0;

            case Category.FourOfAKind:
                return dice.GroupBy(d => d).Any(g => g.Count() >= 4) ? dice.Sum() : 0;

            case Category.FullHouse:
            {
                var groups = dice.GroupBy(d => d).Select(g => g.Count()).OrderBy(c => c).ToList();
                return (groups.Count == 2 && groups[0] == 2 && groups[1] == 3) ? 25 : 0;
            }

            case Category.SmallStraight:
            {
                var set = new HashSet<int>(dice);
                bool ok = (set.Contains(1) && set.Contains(2) && set.Contains(3) && set.Contains(4))
                       || (set.Contains(2) && set.Contains(3) && set.Contains(4) && set.Contains(5))
                       || (set.Contains(3) && set.Contains(4) && set.Contains(5) && set.Contains(6));
                return ok ? 30 : 0;
            }

            case Category.LargeStraight:
            {
                var sorted = dice.Distinct().OrderBy(d => d).ToList();
                bool is12345 = sorted.SequenceEqual(new[] { 1, 2, 3, 4, 5 });
                bool is23456 = sorted.SequenceEqual(new[] { 2, 3, 4, 5, 6 });
                return (is12345 || is23456) ? 40 : 0;
            }

            case Category.Yacht:
                return dice.Distinct().Count() == 1 ? 50 : 0;

            default: return 0;
        }
    }

    public static string GetName(Category cat)
    {
        switch (cat)
        {
            case Category.Ones:          return "에이스";
            case Category.Twos:          return "듀얼";
            case Category.Threes:        return "트리플";
            case Category.Fours:         return "쿼드";
            case Category.Fives:         return "펜타";
            case Category.Sixes:         return "헥사";
            case Category.ThreeOfAKind:  return "쓰리 오브 어 카인드";
            case Category.FourOfAKind:   return "포 오브 어 카인드";
            case Category.FullHouse:     return "풀하우스";
            case Category.SmallStraight: return "스몰 스트레이트";
            case Category.LargeStraight: return "라지 스트레이트";
            case Category.Yacht:         return "요트";
            case Category.Choice:        return "찬스";
            default:                     return "";
        }
    }
}

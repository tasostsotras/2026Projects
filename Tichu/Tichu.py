from __future__ import annotations

import random
from dataclasses import dataclass, field
from collections import Counter
from typing import Optional


# ============================================================
# TICHU
# Terminal implementation for 4 players
#
# Players:
#   0 -- Team A
#   1 -- Team B
#   2 -- Team A
#   3 -- Team B
#
# Seating order:
#   0 -> 1 -> 2 -> 3 -> 0
# ============================================================


SUITS = ["Jade", "Sword", "Pagoda", "Star"]
RANKS = list(range(2, 15))

# Special cards
MAHJONG = "Mah Jong"
DOG = "Dog"
PHOENIX = "Phoenix"
DRAGON = "Dragon"


@dataclass(frozen=True)
class Card:
    name: str
    suit: Optional[str] = None
    rank: Optional[int] = None

    def __str__(self):
        if self.name in {MAHJONG, DOG, PHOENIX, DRAGON}:
            return self.name

        symbols = {
            10: "10",
            11: "J",
            12: "Q",
            13: "K",
            14: "A",
        }

        return f"{symbols.get(self.rank, self.rank)}-{self.suit[0]}"


@dataclass
class Player:
    number: int
    name: str
    team: int
    hand: list[Card] = field(default_factory=list)
    tricks: list[list[Card]] = field(default_factory=list)
    points: int = 0
    grand_tichu: bool = False
    tichu: bool = False

    def sort_hand(self):
        def key(card):
            if card.name == MAHJONG:
                return (0, 0)
            if card.name == DOG:
                return (100, 0)
            if card.name == PHOENIX:
                return (101, 0)
            if card.name == DRAGON:
                return (102, 0)
            return (1, card.rank)

        self.hand.sort(key=key)


@dataclass
class Play:
    cards: list[Card]
    combination: str
    strength: tuple


# ============================================================
# DECK
# ============================================================

def create_deck():
    deck = []

    for suit in SUITS:
        for rank in RANKS:
            deck.append(Card(str(rank), suit, rank))

    deck.extend([
        Card(MAHJONG),
        Card(DOG),
        Card(PHOENIX),
        Card(DRAGON),
    ])

    return deck


# ============================================================
# CARD POINTS
# ============================================================

def card_points(card: Card):
    if card.name == DRAGON:
        return 25

    if card.name == PHOENIX:
        return -25

    if card.rank == 5:
        return 5

    if card.rank == 10:
        return 10

    if card.rank == 13:
        return 10

    return 0


def trick_points(cards):
    return sum(card_points(c) for c in cards)


# ============================================================
# COMBINATION DETECTION
# ============================================================

def is_normal(card):
    return card.rank is not None


def ranks(cards):
    return sorted(c.rank for c in cards if c.rank is not None)


def same_rank(cards):
    return len(cards) > 0 and all(c.rank == cards[0].rank for c in cards)


def is_pair(cards):
    return len(cards) == 2 and same_rank(cards)


def is_triple(cards):
    return len(cards) == 3 and same_rank(cards)


def is_full_house(cards):
    if len(cards) != 5:
        return False

    counts = Counter(ranks(cards))
    return sorted(counts.values()) == [2, 3]


def is_straight(cards):
    if not cards or len(cards) < 5:
        return False

    if any(not is_normal(c) for c in cards):
        return False

    rs = ranks(cards)

    if len(set(rs)) != len(rs):
        return False

    return all(rs[i] + 1 == rs[i + 1]
               for i in range(len(rs) - 1))


def is_pair_sequence(cards):
    if len(cards) < 4 or len(cards) % 2:
        return False

    if any(not is_normal(c) for c in cards):
        return False

    counts = Counter(ranks(cards))

    if any(v != 2 for v in counts.values()):
        return False

    ordered = sorted(counts)

    return all(ordered[i] + 1 == ordered[i + 1]
               for i in range(len(ordered) - 1))


def bomb_type(cards):
    # Four of a kind
    if len(cards) == 4 and all(is_normal(c) for c in cards):
        if same_rank(cards):
            return ("four", cards[0].rank)

    # Straight flush
    if len(cards) >= 5:
        if all(is_normal(c) for c in cards):
            if len({c.suit for c in cards}) == 1 and is_straight(cards):
                return ("straight_flush", max(ranks(cards)))

    return None


def classify(cards):
    """
    Returns:
        (combination_name, strength_tuple)

    Higher strength means a stronger combination of the same type.
    """
    if not cards:
        return None

    b = bomb_type(cards)
    if b:
        return b

    if len(cards) == 1:
        card = cards[0]

        if card.name == PHOENIX:
            return ("single_phoenix", (14.5,))

        if card.name == MAHJONG:
            return ("single", (1,))

        if card.name == DOG:
            return ("dog", (0,))

        if card.name == DRAGON:
            return ("single", (15,))

        return ("single", (card.rank,))

    if len(cards) == 2 and is_pair(cards):
        return ("pair", (cards[0].rank,))

    if len(cards) == 3 and is_triple(cards):
        return ("triple", (cards[0].rank,))

    if len(cards) == 5 and is_full_house(cards):
        counts = Counter(ranks(cards))
        triple_rank = max(k for k, v in counts.items() if v == 3)
        return ("full_house", (triple_rank,))

    if is_pair_sequence(cards):
        return ("pair_sequence", (len(cards), max(ranks(cards))))

    if is_straight(cards):
        return ("straight", (len(cards), max(ranks(cards))))

    return None


# ============================================================
# PLAY VALIDATION
# ============================================================

def remove_cards(hand, cards):
    new_hand = hand.copy()

    for card in cards:
        try:
            new_hand.remove(card)
        except ValueError:
            return None

    return new_hand


def beats(play: Play, current: Optional[Play]):
    if current is None:
        return True

    # Bombs beat non-bombs.
    bomb = play.combination in {"four", "straight_flush"}
    current_bomb = current.combination in {"four", "straight_flush"}

    if bomb and not current_bomb:
        return True

    if not bomb and current_bomb:
        return False

    if bomb and current_bomb:
        if play.combination == "straight_flush" and current.combination == "four":
            return True

        if play.combination == "four" and current.combination == "straight_flush":
            return False

        return play.strength > current.strength

    # Phoenix behaves specially when played as a single.
    if play.combination == "single_phoenix":
        return current.combination == "single" and current.strength[0] < 14

    if current.combination == "single_phoenix":
        return True

    if play.combination != current.combination:
        return False

    return play.strength > current.strength


# ============================================================
# INPUT / DISPLAY
# ============================================================

def print_hand(player):
    print("\nYour hand:")
    for i, card in enumerate(player.hand):
        print(f"{i:2}: {card}")


def parse_indexes(text):
    if not text.strip():
        return []

    try:
        return [int(x) for x in text.replace(",", " ").split()]
    except ValueError:
        return None


def choose_cards(player, current_play):
    while True:
        print_hand(player)

        if current_play:
            print(
                f"\nCurrent trick: "
                f"{' '.join(map(str, current_play.cards))}"
            )

        print("\nEnter card indexes to play.")
        print("Enter 'p' to pass.")

        answer = input("> ").strip().lower()

        if answer == "p":
            return None

        indexes = parse_indexes(answer)

        if indexes is None:
            print("Invalid input.")
            continue

        if len(indexes) != len(set(indexes)):
            print("Do not repeat indexes.")
            continue

        if any(i < 0 or i >= len(player.hand) for i in indexes):
            print("Invalid card index.")
            continue

        cards = [player.hand[i] for i in indexes]

        classification = classify(cards)

        if classification is None:
            print("Those cards do not form a valid combination.")
            continue

        combination, strength = classification
        play = Play(cards, combination, strength)

        if current_play and not beats(play, current_play):
            print("That play does not beat the current play.")
            continue

        return cards


# ============================================================
# TRADING
# ============================================================

def trading_phase(players):
    print("\n" + "=" * 60)
    print("TRADING PHASE")
    print("=" * 60)

    trades = {}

    for player in players:
        print(f"\n{player.name}, choose 3 cards to trade.")

        while True:
            print_hand(player)
            answer = input("Indexes of 3 cards: ")

            indexes = parse_indexes(answer)

            if (
                indexes is not None
                and len(indexes) == 3
                and len(set(indexes)) == 3
                and all(0 <= i < len(player.hand) for i in indexes)
            ):
                break

            print("Please enter exactly 3 valid indexes.")

        trades[player.number] = [
            player.hand[i] for i in indexes
        ]

    # Tichu trading:
    # each player gives one card to left, partner, and right.
    for player in players:
        selected = trades[player.number]

        left = (player.number + 1) % 4
        partner = (player.number + 2) % 4
        right = (player.number + 3) % 4

        # We use:
        # card 0 -> left
        # card 1 -> partner
        # card 2 -> right
        players[left].hand.append(selected[0])
        players[partner].hand.append(selected[1])
        players[right].hand.append(selected[2])

    # Remove the traded cards.
    for player in players:
        for card in trades[player.number]:
            player.hand.remove(card)

        player.sort_hand()


# ============================================================
# TICHU CALLS
# ============================================================

def tichu_calls(players):
    print("\n" + "=" * 60)
    print("TICHU CALLS")
    print("=" * 60)

    for player in players:
        print_hand(player)

        answer = input(
            f"{player.name}: call Tichu? [y/N] "
        ).strip().lower()

        player.tichu = answer == "y"

        if player.tichu:
            print(f"{player.name} called Tichu!")


# ============================================================
# GRAND TICHU
# ============================================================

def grand_tichu_phase(players):
    """
    Simplified Grand Tichu decision before the final cards are dealt.
    """

    print("\n" + "=" * 60)
    print("GRAND TICHU")
    print("=" * 60)

    for player in players:
        print(f"\n{player.name}'s first 8 cards:")
        print_hand(player)

        answer = input(
            f"{player.name}: call Grand Tichu? [y/N] "
        ).strip().lower()

        player.grand_tichu = answer == "y"

        if player.grand_tichu:
            print(f"{player.name} called GRAND TICHU!")


# ============================================================
# LEADING PLAYER
# ============================================================

def find_mahjong_player(players):
    for p in players:
        if any(c.name == MAHJONG for c in p.hand):
            return p.number

    raise RuntimeError("Mah Jong not found")


# ============================================================
# MAHJONG REQUEST
# ============================================================

def mahjong_request(players, player):
    if not any(c.name == MAHJONG for c in player.hand):
        return

    print(
        f"\n{player.name} played Mah Jong."
    )

    while True:
        answer = input(
            "Request a rank from 2-14, or press Enter for none: "
        ).strip()

        if not answer:
            return

        try:
            rank = int(answer)

            if 2 <= rank <= 14:
                print(
                    f"{player.name} requests a {rank}."
                )
                return rank
        except ValueError:
            pass

        print("Please enter a number from 2 to 14.")


# ============================================================
# TRICK PLAY
# ============================================================

def play_trick(players, leader):
    print("\n" + "=" * 60)
    print("NEW TRICK")
    print("=" * 60)

    current_play = None
    current_player = leader
    passes = 0

    trick_cards = []

    while True:
        player = players[current_player]

        print(f"\n--- {player.name}'s turn ---")

        cards = choose_cards(player, current_play)

        if cards is None:
            print(f"{player.name} passes.")
            passes += 1

        else:
            passes = 0

            classification = classify(cards)
            combination, strength = classification

            # Dog cannot be played except as a lead.
            if (
                any(c.name == DOG for c in cards)
                and current_play is not None
            ):
                print("The Dog can only be played when leading.")
                continue

            # Remove cards.
            new_hand = remove_cards(player.hand, cards)

            if new_hand is None:
                print("You don't have those cards.")
                continue

            player.hand = new_hand
            player.sort_hand()

            current_play = Play(
                cards,
                combination,
                strength
            )

            trick_cards.extend(cards)

            print(
                f"{player.name} plays: "
                f"{' '.join(map(str, cards))}"
            )

            if any(c.name == MAHJONG for c in cards):
                mahjong_request(players, player)

            # Dog immediately transfers the lead to partner.
            if any(c.name == DOG for c in cards):
                partner = (player.number + 2) % 4

                print(
                    f"The Dog gives the lead to "
                    f"{players[partner].name}."
                )

                return partner, trick_cards

        # Four players have passed after a play.
        if current_play and passes >= 3:
            winner = current_player

            # The winner is the last player who played,
            # not necessarily current_player after increment.
            return winner, trick_cards

        current_player = (current_player + 1) % 4

        # If only one player remains with cards, end round.
        if len(player.hand) == 0:
            return player.number, trick_cards


# ============================================================
# ROUND
# ============================================================

def deal_initial(players):
    deck = create_deck()
    random.shuffle(deck)

    for p in players:
        p.hand.clear()
        p.tricks.clear()
        p.points = 0
        p.grand_tichu = False
        p.tichu = False

    # First 8 cards.
    for _ in range(8):
        for p in players:
            p.hand.append(deck.pop())

    for p in players:
        p.sort_hand()

    return deck


def deal_remaining(players, deck):
    while deck:
        for p in players:
            if deck:
                p.hand.append(deck.pop())

    for p in players:
        p.sort_hand()


def score_tricks(players):
    for p in players:
        p.points = sum(
            trick_points(trick)
            for trick in p.tricks
        )


def apply_tichu_scoring(players):
    for p in players:
        if p.tichu:
            # If the player has no cards left, they succeeded.
            success = len(p.hand) == 0

            if success:
                p.points += 100
            else:
                p.points -= 100

        if p.grand_tichu:
            success = len(p.hand) == 0

            if success:
                p.points += 200
            else:
                p.points -= 200


def double_win_check(players):
    """
    If both players on one team go out before either opponent,
    that team gets 200 points.

    Returns:
        0 / 1 if double victory occurred
        None otherwise
    """

    # This is determined from finishing order.
    finished = sorted(
        players,
        key=lambda p: len(p.hand)
    )

    if len(finished) >= 2:
        if finished[0].team == finished[1].team:
            return finished[0].team

    return None


def run_round(players, scores):
    print("\n\n" + "#" * 70)
    print("NEW TICHU ROUND")
    print("#" * 70)

    deck = deal_initial(players)

    # Grand Tichu is declared after 8 cards.
    grand_tichu_phase(players)

    # Deal remaining cards.
    deal_remaining(players, deck)

    # Normal Tichu declarations.
    tichu_calls(players)

    # Trading.
    trading_phase(players)

    leader = find_mahjong_player(players)

    print(
        f"\n{players[leader].name} has the Mah Jong "
        f"and leads the first trick."
    )

    finished = set()

    while len(finished) < 3:
        winner, trick_cards = play_trick(players, leader)

        players[winner].tricks.append(trick_cards)

        print(
            f"\n{players[winner].name} wins the trick."
        )

        # A player is finished when they have no cards.
        for p in players:
            if not p.hand:
                finished.add(p.number)

        leader = winner

        # If only one player has cards, remaining cards
        # go to the trick winner according to the simplified
        # implementation.
        if len(finished) == 3:
            remaining_player = next(
                p for p in players if p.hand
            )

            # Remaining cards count as captured by the winner
            # of the final trick.
            players[leader].tricks.append(
                remaining_player.hand.copy()
            )

            remaining_player.hand.clear()

    score_tricks(players)

    # Double victory.
    double_team = double_win_check(players)

    if double_team is not None:
        print(
            f"\nTEAM {double_team + 1} achieved a "
            f"double victory!"
        )
        scores[double_team] += 200
        return

    apply_tichu_scoring(players)

    for team in range(2):
        team_points = sum(
            p.points
            for p in players
            if p.team == team
        )

        scores[team] += team_points

    print("\nROUND RESULTS")
    print("-" * 40)

    for p in players:
        print(
            f"{p.name}: {p.points:+d} points"
        )

    print("\nTOTAL SCORE")
    print("-" * 40)

    print(f"Team 1: {scores[0]}")
    print(f"Team 2: {scores[1]}")


# ============================================================
# MAIN GAME
# ============================================================

def main():
    print("=" * 70)
    print("TICHU - PYTHON TERMINAL EDITION")
    print("=" * 70)

    names = []

    for i in range(4):
        name = input(
            f"Enter name for Player {i + 1} "
            f"(default P{i + 1}): "
        ).strip()

        if not name:
            name = f"P{i + 1}"

        names.append(name)

    players = [
        Player(0, names[0], 0),
        Player(1, names[1], 1),
        Player(2, names[2], 0),
        Player(3, names[3], 1),
    ]

    scores = [0, 0]

    while True:
        run_round(players, scores)

        if scores[0] >= 1000 or scores[1] >= 1000:
            break

        print("\n" + "=" * 70)
        print("CURRENT SCORE")
        print("=" * 70)
        print(f"Team 1: {scores[0]}")
        print(f"Team 2: {scores[1]}")

        answer = input(
            "\nPlay another round? [Y/n] "
        ).strip().lower()

        if answer == "n":
            break

    print("\n" + "=" * 70)
    print("GAME OVER")
    print("=" * 70)

    print(f"Team 1: {scores[0]}")
    print(f"Team 2: {scores[1]}")

    print("\nThanks for playing!")


if __name__ == "__main__":
    main()

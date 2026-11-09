(() => {
    const passwordInput = document.getElementById("Input_Password");
    const scoreValue = document.getElementById("password-score-value");
    const scoreLabel = document.getElementById("password-score-label");
    const scoreMeter = document.getElementById("password-score-meter");
    if (!passwordInput || !scoreValue || !scoreLabel || !scoreMeter) return;

    function updateScore() {
        const password = passwordInput.value;
        // Brug samme længde, tegnkategorier og vægte som PasswordPolicy.Score.
        const characters = password.split("");
        let score = Math.min(50, password.length * 4);
        if (characters.some(character => /\p{Lu}/u.test(character))) score += 10;
        if (characters.some(character => /\p{Ll}/u.test(character))) score += 10;
        if (characters.some(character => /\p{Nd}/u.test(character))) score += 15;
        if (characters.some(character => !/[\p{L}\p{Nd}]/u.test(character))) score += 15;
        score = Math.min(100, score);

        scoreValue.textContent = `${score}/100`;
        scoreLabel.textContent = !password ? "Skriv en adgangskode" : score < 50 ? "svag" : score < 80 ? "rimelig" : "stærk";
        scoreMeter.value = score;
        scoreMeter.textContent = `${score}/100`;
    }

    passwordInput.addEventListener("input", updateScore);
    window.addEventListener("pageshow", updateScore);
    updateScore();
})();

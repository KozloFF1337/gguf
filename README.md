@RenderBody()

<!-- ВСТАВЬ ЭТОТ КОД ПЕРЕД @RenderBody() -->
<div class="dev-warning-banner">
    <div class="warning-content">
        <span class="warning-icon">⚠️</span>
        <span class="warning-text">
            <strong>ВНИМАНИЕ:</strong> Проект находится в стадии разработки. Данные не прошли верификацию и могут содержать ошибки.
        </span>
    </div>
</div>

@RenderBody()


/* Плашка предупреждения */
.dev-warning-banner {
    width: 100%;
    background: linear-gradient(135deg, #ff6b6b 0%, #fd817e 100%);
    color: white;
    position: fixed;
    top: 60px; /* Сразу после header, который 60px высотой */
    z-index: 999;
    box-shadow: 0 4px 12px rgba(253, 129, 126, 0.4);
    border-bottom: 2px solid rgba(255, 255, 255, 0.2);
    animation: slideDown 0.4s ease-out;
}

.warning-content {
    max-width: 1200px;
    margin: 0 auto;
    padding: 14px 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 12px;
}

.warning-icon {
    font-size: 24px;
    animation: pulse 2s infinite;
}

.warning-text {
    font-family: 'Montserrat', Arial, sans-serif;
    font-size: 15px;
    font-weight: 500;
    letter-spacing: 0.3px;
    line-height: 1.4;
}

.warning-text strong {
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.8px;
}

/* Анимации */
@keyframes slideDown {
    from {
        transform: translateY(-100%);
        opacity: 0;
    }
    to {
        transform: translateY(0);
        opacity: 1;
    }
}

@keyframes pulse {
    0%, 100% {
        transform: scale(1);
    }
    50% {
        transform: scale(1.15);
    }
}

/* Корректировка отступа для контента */
.container {
    min-height: calc(90vh);
    padding-top: 112px; /* 60px header + ~52px warning banner */
}

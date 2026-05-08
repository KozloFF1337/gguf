/* Кнопка-ссылка скачивания руководства пользователя в шапке */
.manual-btn,
.manual-btn:link,
.manual-btn:visited {
    margin-left: auto;
    margin-right: 18px;
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 8px 16px;
    color: #fd817e;
    background: rgba(253, 129, 126, 0.08);
    border: 1px solid rgba(253, 129, 126, 0.45);
    border-radius: 6px;
    text-decoration: none;
    font-weight: 600;
    font-size: 14px;
    white-space: nowrap;
    transition: background-color 0.2s ease, border-color 0.2s ease, color 0.2s ease, box-shadow 0.2s ease;
    box-shadow: 0 0 0 0 rgba(253, 129, 126, 0);
}

.manual-btn:hover,
.manual-btn:focus,
.manual-btn:active {
    color: #fff;
    background: #fd817e;
    border-color: #fd817e;
    box-shadow: 0 0 12px rgba(253, 129, 126, 0.45);
    text-decoration: none;
}

.manual-btn .manual-icon {
    flex-shrink: 0;
}

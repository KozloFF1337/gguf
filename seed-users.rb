# =====================================================================
# Идемпотентный сидинг GitLab: админ + read-only пользователь + группа.
# Запускается внутри контейнера через gitlab-rails runner (см. load-and-run.sh).
# Повторный запуск безопасен — обновит пароли, не наплодит дублей.
#
#   docker exec -e ADMIN_PASSWORD=... -e USER_PASSWORD=... \
#     [-e ADMIN_USERNAME=...] [-e USER_USERNAME=...] \
#     gitlab gitlab-rails runner /tmp/seed-users.rb
#
# ВАЖНО про имена: GitLab резервирует ряд логинов (admin, api, users, root …).
# Логин 'admin' создать НЕЛЬЗЯ — поэтому дефолт админа = 'administrator'.
# Полноправный супер-пользователь 'root' и так есть (пароль = GITLAB_ROOT_PASSWORD).
# =====================================================================

admin_password = ENV['ADMIN_PASSWORD']
user_password  = ENV['USER_PASSWORD']
abort('ADMIN_PASSWORD не задан') if admin_password.to_s.empty?
abort('USER_PASSWORD не задан')  if user_password.to_s.empty?

# Зарезервированные/проблемные логины верхнего уровня в GitLab.
RESERVED = %w[admin api root users dashboard groups projects help profile explore search public uploads].freeze

admin_username = (ENV['ADMIN_USERNAME'].to_s.strip.empty? ? 'administrator' : ENV['ADMIN_USERNAME'].strip)
user_username  = (ENV['USER_USERNAME'].to_s.strip.empty?  ? 'user'          : ENV['USER_USERNAME'].strip)

if RESERVED.include?(admin_username.downcase)
  warn "! Логин '#{admin_username}' зарезервирован GitLab — использую 'administrator'."
  admin_username = 'administrator'
end
if RESERVED.include?(user_username.downcase)
  warn "! Логин '#{user_username}' зарезервирован GitLab — использую 'altair-user'."
  user_username = 'altair-user'
end

def upsert_user(username:, name:, email:, password:, admin:)
  u = User.find_by(username: username) || User.new
  u.username              = username
  u.name                  = name
  u.email                 = email
  u.password              = password
  u.password_confirmation = password
  u.admin                 = admin
  u.skip_confirmation!                 # почта сразу подтверждена
  u.password_automatically_set = false # не требовать смены пароля при входе
  if u.save
    u
  else
    warn "! Не удалось сохранить '#{username}': #{u.errors.full_messages.join('; ')}"
    nil
  end
end

admin = upsert_user(username: admin_username, name: 'Administrator',
                    email: "#{admin_username}@altair.local", password: admin_password, admin: true)
puts "✓ админ:        #{admin_username}  (полные права)" if admin

ro = upsert_user(username: user_username, name: 'Read Only',
                 email: "#{user_username}@altair.local", password: user_password, admin: false)
puts "✓ пользователь: #{user_username}  (обычная учётка)" if ro

# Владелец группы — созданный админ; если не получилось, берём root.
owner = admin || User.find_by(username: 'root')

# --- Группа 'altair': кладёшь проект сюда, и user сразу видит его read-only ---
group = Group.find_by(full_path: 'altair')
unless group
  res = Groups::CreateService.new(
    owner,
    name: 'Altair',
    path: 'altair',
    visibility_level: Gitlab::VisibilityLevel::PRIVATE
  ).execute
  group = res.respond_to?(:payload) ? res.payload[:group] : res
end

if group&.persisted?
  group.add_owner(owner)            if owner
  group.add_reporter(ro)            if ro    # Reporter: клонировать/смотреть, БЕЗ push
  puts "✓ группа 'altair': owner=#{owner&.username}, reporter=#{ro&.username || '—'}"
else
  warn "! Группу 'altair' создать не удалось — заведи её вручную и добавь #{user_username} как Reporter."
end

puts
puts 'Готово. Создай ПУСТОЙ проект в группе altair (UI) и залей код:'
puts '  git remote add origin http://<host>:<port>/altair/altair.git'
puts '  git push -u origin --all'

local core = {}

local function getGameID()
    local address = 0x0
    return ReadValueString(address, 6)
end
core.getGameID = getGameID

local function getPlayerbase()
  local pointer = 0
  if getGameID() == "RMCP01" then playerbase = 0x9C18F8
  elseif getGameID() == "RMCE01" then playerbase = 0x9BD110
  end
  local offset1 = 0x20
  local offset2 = 0x0
  local offset3 = 0x10
  local offset4 = 0x10
  local address1 = GetPointerNormal(playerbase)
  local address2 = GetPointerNormal(address1 + offset1)
  local address3 = GetPointerNormal(address2 + offset2)
  local address4 = GetPointerNormal(address3 + offset3)
  return ReadValue32(address4 + offset4)
end
core.getPlayerbase = getPlayerbase

local function getXpos()
  local pointer = 0x9C2EF8
  local offset1 = 0x40
  local offset2 = 0x0
  local address1 = GetPointerNormal(pointer)
  local address2 = GetPointerNormal(address1 + offset1)
  return ReadValueFloat(address2 + offset2)
end
core.getXpos = getXpos

local function getYpos()
  local pointer = 0x9C2EF8
  local offset1 = 0x40
  local offset2 = 0x4
  local address1 = GetPointerNormal(pointer)
  local address2 = GetPointerNormal(address1 + offset1)
  return ReadValueFloat(address2 + offset2)
end
core.getYpos = getYpos

local function getZpos()
  local pointer = 0x9C2EF8
  local offset1 = 0x40
  local offset2 = 0x8
  local address1 = GetPointerNormal(pointer)
  local address2 = GetPointerNormal(address1 + offset1)
  return ReadValueFloat(address2 + offset2)
end
core.getZpos = getZpos

local function getFrameOfInput()
  local frameaddress = 0x0
  if getGameID() == "RMCP01" then frameaddress = 0x9C38C2
  elseif getGameID() == "RMCE01" then frameaddress = 0x9BF0BA
  end
  return ReadValue16(frameaddress)
end
core.getFrameOfInput = getFrameOfInput

local function getButtonInput()
  local pointer = 0
  if getGameID() == "RMCP01" then pointer = 0x42E324
  elseif getGameID() == "RMCE01" then pointer = 0x429F14
  elseif getGameID() == "RMCJ01" then pointer = 0x42DC14
  elseif getGameID() == "RMCK01" then pointer = 0x41C2B4
  end
  local offset = 0x2841
  local address = GetPointerNormal(pointer)
  return ReadValue8(address + offset)
end
core.getButtonInput = getButtonInput

local function getDPADInput()
  local pointer = 0
  if getGameID() == "RMCP01" then pointer = 0x42E324
  elseif getGameID() == "RMCE01" then pointer = 0x429F14
  elseif getGameID() == "RMCJ01" then pointer = 0x42DC14
  elseif getGameID() == "RMCK01" then pointer = 0x41C2B4
  end
  local offset = 0x284F
  local address = GetPointerNormal(pointer)
  return ReadValue8(address + offset)
end
core.getDPADInput = getDPADInput

local function getHorizInput()
  local pointer = 0
  if getGameID() == "RMCP01" then pointer = 0x42E324
  elseif getGameID() == "RMCE01" then pointer = 0x429F14
  elseif getGameID() == "RMCJ01" then pointer = 0x42DC14
  elseif getGameID() == "RMCK01" then pointer = 0x41C2B4
  end
  local offset = 0x284C
  local address = GetPointerNormal(pointer)
  return ReadValue8(address + offset)
end
core.getHorizInput = getHorizInput

local function getVertInput()
  local pointer = 0
  if getGameID() == "RMCP01" then pointer = 0x42E324
  elseif getGameID() == "RMCE01" then pointer = 0x429F14
  elseif getGameID() == "RMCJ01" then pointer = 0x42DC14
  elseif getGameID() == "RMCK01" then pointer = 0x41C2B4
  end
  local offset = 0x284D
  local address = GetPointerNormal(pointer)
  return ReadValue8(address + offset)
end
core.getVertInput = getVertInput
return core